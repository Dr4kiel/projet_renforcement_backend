using server.DTOs.Of;
using server.Models;
using server.Repositories.Interfaces;
using server.Services.Interfaces;

namespace server.Services;

public class OfService : IOfService
{
    private readonly IOfRepository _ofRepository;

    public OfService(IOfRepository ofRepository)
    {
        _ofRepository = ofRepository;
    }

    public async Task<IEnumerable<OfDto>> GetAllOfsAsync()
    {
        var ofs = await _ofRepository.GetAllWithLinesCountAsync();
        return ofs.Select(MapToDto);
    }

    public async Task<OfDto?> GetOfByIdAsync(int id)
    {
        var of = await _ofRepository.GetByIdWithLinesAsync(id);
        return of == null ? null : MapToDto(of);
    }

    public async Task<OfDto> CreateOfAsync(CreateOfRequestDto request)
    {
        // Validate OF name uniqueness
        if (await _ofRepository.OfNameExistsAsync(request.Of))
        {
            throw new InvalidOperationException("OF name already exists");
        }

        // Validate QteProduite <= QteTotale
        if (request.QteProduite > request.QteTotale)
        {
            throw new InvalidOperationException("QteProduite cannot exceed QteTotale");
        }

        var of = new Of
        {
            Of_ = request.Of,
            Produit = request.Produit,
            QteProduite = request.QteProduite,
            QteTotale = request.QteTotale
        };

        var createdOf = await _ofRepository.CreateAsync(of);
        var ofWithLines = await _ofRepository.GetByIdWithLinesAsync(createdOf.Id);
        return MapToDto(ofWithLines!);
    }

    public async Task<OfDto?> UpdateOfAsync(int id, UpdateOfRequestDto request)
    {
        var of = await _ofRepository.GetByIdAsync(id);
        if (of == null)
        {
            return null;
        }

        // Validate OF name uniqueness if changed
        if (request.Of != null && request.Of != of.Of_)
        {
            if (await _ofRepository.OfNameExistsAsync(request.Of, id))
            {
                throw new InvalidOperationException("OF name already exists");
            }

            of.Of_ = request.Of;
        }

        if (request.Produit != null)
        {
            of.Produit = request.Produit;
        }

        if (request.QteProduite.HasValue)
        {
            of.QteProduite = request.QteProduite.Value;
        }

        if (request.QteTotale.HasValue)
        {
            of.QteTotale = request.QteTotale.Value;
        }

        // Validate QteProduite <= QteTotale after updates
        if (of.QteProduite > of.QteTotale)
        {
            throw new InvalidOperationException("QteProduite cannot exceed QteTotale");
        }

        await _ofRepository.UpdateAsync(of);
        var updatedOf = await _ofRepository.GetByIdWithLinesAsync(id);
        return MapToDto(updatedOf!);
    }

    public async Task<bool> DeleteOfAsync(int id)
    {
        // Check if OF has associated lines
        if (await _ofRepository.HasLinesAsync(id))
        {
            throw new InvalidOperationException("Cannot delete OF that has associated lines");
        }

        return await _ofRepository.DeleteAsync(id);
    }

    private static OfDto MapToDto(Of of)
    {
        return new OfDto
        {
            Id = of.Id,
            Of = of.Of_,
            Produit = of.Produit,
            QteProduite = of.QteProduite,
            QteTotale = of.QteTotale,
            LinesEnCoursCount = of.LinesEnCours?.Count ?? 0,
            LinesSuivantCount = of.LinesSuivant?.Count ?? 0
        };
    }
}
