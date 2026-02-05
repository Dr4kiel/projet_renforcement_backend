import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { ofsService } from '../../services/ofs.service';
import type { CreateOfRequestDto, UpdateOfRequestDto } from '../../types/api';

export const OfFormPage = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const isEdit = !!id;

  const [formData, setFormData] = useState({
    of: '',
    produit: '',
    qteProduite: 0,
    qteTotale: 1,
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (isEdit) {
      loadOf();
    }
  }, [id]);

  const loadOf = async () => {
    try {
      const of = await ofsService.getById(Number(id));
      setFormData({
        of: of.of,
        produit: of.produit,
        qteProduite: of.qteProduite,
        qteTotale: of.qteTotale,
      });
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erreur lors du chargement');
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);

    try {
      if (isEdit) {
        const updateData: UpdateOfRequestDto = {
          of: formData.of,
          produit: formData.produit,
          qteProduite: Number(formData.qteProduite),
          qteTotale: Number(formData.qteTotale),
        };
        await ofsService.update(Number(id), updateData);
      } else {
        const createData: CreateOfRequestDto = {
          of: formData.of,
          produit: formData.produit,
          qteProduite: Number(formData.qteProduite),
          qteTotale: Number(formData.qteTotale),
        };
        await ofsService.create(createData);
      }
      navigate('/backoffice/ofs');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erreur lors de la sauvegarde');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="bg-white shadow rounded-lg p-6">
      <h2 className="text-2xl font-bold text-gray-900 mb-6">
        {isEdit ? 'Modifier l\'OF' : 'Nouvel OF'}
      </h2>

      {error && (
        <div className="bg-red-50 text-red-600 p-4 rounded-md mb-4">{error}</div>
      )}

      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="block text-sm font-medium text-gray-700">Numéro d'OF</label>
          <input
            type="text"
            required
            maxLength={100}
            value={formData.of}
            onChange={(e) => setFormData({ ...formData, of: e.target.value })}
            className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 px-3 py-2 border"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700">Produit</label>
          <input
            type="text"
            required
            maxLength={200}
            value={formData.produit}
            onChange={(e) => setFormData({ ...formData, produit: e.target.value })}
            className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 px-3 py-2 border"
          />
        </div>

        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700">Quantité produite</label>
            <input
              type="number"
              min="0"
              required
              value={formData.qteProduite}
              onChange={(e) => setFormData({ ...formData, qteProduite: Number(e.target.value) })}
              className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 px-3 py-2 border"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700">Quantité totale</label>
            <input
              type="number"
              min="1"
              required
              value={formData.qteTotale}
              onChange={(e) => setFormData({ ...formData, qteTotale: Number(e.target.value) })}
              className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 px-3 py-2 border"
            />
          </div>
        </div>

        <div className="bg-blue-50 p-4 rounded-md">
          <p className="text-sm text-blue-700">
            Progression: {formData.qteTotale > 0 ? ((formData.qteProduite / formData.qteTotale) * 100).toFixed(1) : 0}%
          </p>
        </div>

        <div className="flex justify-end space-x-3 pt-4">
          <button
            type="button"
            onClick={() => navigate('/backoffice/ofs')}
            className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 hover:bg-gray-50"
          >
            Annuler
          </button>
          <button
            type="submit"
            disabled={loading}
            className="px-4 py-2 bg-blue-600 text-white rounded-md text-sm font-medium hover:bg-blue-700 disabled:opacity-50"
          >
            {loading ? 'Sauvegarde...' : 'Sauvegarder'}
          </button>
        </div>
      </form>
    </div>
  );
};
