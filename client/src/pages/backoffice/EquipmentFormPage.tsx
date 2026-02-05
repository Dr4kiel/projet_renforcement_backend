import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { equipmentsService } from '../../services/equipments.service';
import { tagsService } from '../../services/tags.service';
import type { CreateEquipmentRequestDto, UpdateEquipmentRequestDto, TagDto } from '../../types/api';

export const EquipmentFormPage = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const isEdit = !!id;

  const [formData, setFormData] = useState({
    name: '',
    tagIds: [] as number[],
  });
  const [tags, setTags] = useState<TagDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadTags();
    if (isEdit) {
      loadEquipment();
    }
  }, [id]);

  const loadTags = async () => {
    try {
      const data = await tagsService.getAll();
      setTags(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erreur lors du chargement des tags');
    }
  };

  const loadEquipment = async () => {
    try {
      const equipment = await equipmentsService.getById(Number(id));
      setFormData({
        name: equipment.name,
        tagIds: equipment.tags?.map(t => t.id) || [],
      });
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erreur lors du chargement');
    }
  };

  const handleTagToggle = (tagId: number) => {
    setFormData(prev => ({
      ...prev,
      tagIds: prev.tagIds.includes(tagId)
        ? prev.tagIds.filter(id => id !== tagId)
        : [...prev.tagIds, tagId]
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);

    try {
      if (isEdit) {
        const updateData: UpdateEquipmentRequestDto = {
          name: formData.name,
          tagIds: formData.tagIds,
        };
        await equipmentsService.update(Number(id), updateData);
      } else {
        const createData: CreateEquipmentRequestDto = {
          name: formData.name,
          tagIds: formData.tagIds,
        };
        await equipmentsService.create(createData);
      }
      navigate('/backoffice/equipments');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erreur lors de la sauvegarde');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="bg-white shadow rounded-lg p-6">
      <h2 className="text-2xl font-bold text-gray-900 mb-6">
        {isEdit ? 'Modifier l\'équipement' : 'Nouvel équipement'}
      </h2>

      {error && (
        <div className="bg-red-50 text-red-600 p-4 rounded-md mb-4">{error}</div>
      )}

      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="block text-sm font-medium text-gray-700">Nom de l'équipement</label>
          <input
            type="text"
            required
            maxLength={100}
            value={formData.name}
            onChange={(e) => setFormData({ ...formData, name: e.target.value })}
            className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 px-3 py-2 border"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">Tags associés</label>
          <div className="border border-gray-300 rounded-md p-4 max-h-60 overflow-y-auto">
            {tags.map((tag) => (
              <div key={tag.id} className="flex items-center mb-2">
                <input
                  type="checkbox"
                  id={`tag-${tag.id}`}
                  checked={formData.tagIds.includes(tag.id)}
                  onChange={() => handleTagToggle(tag.id)}
                  className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
                />
                <label htmlFor={`tag-${tag.id}`} className="ml-2 text-sm text-gray-700">
                  {tag.tagName}
                </label>
              </div>
            ))}
          </div>
        </div>

        <div className="flex justify-end space-x-3 pt-4">
          <button
            type="button"
            onClick={() => navigate('/backoffice/equipments')}
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
