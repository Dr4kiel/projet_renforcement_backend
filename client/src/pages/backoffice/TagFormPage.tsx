import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { tagsService } from '../../services/tags.service';
import type { CreateTagRequestDto, UpdateTagRequestDto } from '../../types/api';

export const TagFormPage = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const isEdit = !!id;

  const [formData, setFormData] = useState({
    tagName: '',
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (isEdit) {
      loadTag();
    }
  }, [id]);

  const loadTag = async () => {
    try {
      const tag = await tagsService.getById(Number(id));
      setFormData({
        tagName: tag.tagName,
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
        const updateData: UpdateTagRequestDto = { tagName: formData.tagName };
        await tagsService.update(Number(id), updateData);
      } else {
        const createData: CreateTagRequestDto = { tagName: formData.tagName };
        await tagsService.create(createData);
      }
      navigate('/backoffice/tags');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erreur lors de la sauvegarde');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="bg-white shadow rounded-lg p-6">
      <h2 className="text-2xl font-bold text-gray-900 mb-6">
        {isEdit ? 'Modifier le tag' : 'Nouveau tag'}
      </h2>

      {error && (
        <div className="bg-red-50 text-red-600 p-4 rounded-md mb-4">{error}</div>
      )}

      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="block text-sm font-medium text-gray-700">Nom du tag</label>
          <input
            type="text"
            required
            maxLength={100}
            value={formData.tagName}
            onChange={(e) => setFormData({ ...formData, tagName: e.target.value })}
            className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 px-3 py-2 border"
            placeholder="Ex: TEMP_LIGNE_1, PRESSURE_SENSOR_A"
          />
          <p className="mt-1 text-sm text-gray-500">
            Identifiant unique pour le capteur
          </p>
        </div>

        <div className="flex justify-end space-x-3 pt-4">
          <button
            type="button"
            onClick={() => navigate('/backoffice/tags')}
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
