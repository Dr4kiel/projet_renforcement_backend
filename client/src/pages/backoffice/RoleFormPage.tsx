import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { rolesService } from '../../services/roles.service';
import type { CreateRoleRequestDto, UpdateRoleRequestDto } from '../../types/api';

export const RoleFormPage = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const isEdit = !!id;

  const [formData, setFormData] = useState({
    name: '',
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (isEdit) {
      loadRole();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id]);

  const loadRole = async () => {
    try {
      const role = await rolesService.getById(Number(id));
      setFormData({
        name: role.name,
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
        const updateData: UpdateRoleRequestDto = { name: formData.name };
        await rolesService.update(Number(id), updateData);
      } else {
        const createData: CreateRoleRequestDto = { name: formData.name };
        await rolesService.create(createData);
      }
      navigate('/backoffice/roles');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erreur lors de la sauvegarde');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="bg-white shadow rounded-lg p-6">
      <h2 className="text-2xl font-bold text-gray-900 mb-6">
        {isEdit ? 'Modifier le rôle' : 'Nouveau rôle'}
      </h2>

      {error && (
        <div className="bg-red-50 text-red-600 p-4 rounded-md mb-4">{error}</div>
      )}

      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="block text-sm font-medium text-gray-700">Nom du rôle</label>
          <input
            type="text"
            required
            maxLength={50}
            value={formData.name}
            onChange={(e) => setFormData({ ...formData, name: e.target.value })}
            className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 px-3 py-2 border"
          />
        </div>

        <div className="flex justify-end space-x-3 pt-4">
          <button
            type="button"
            onClick={() => navigate('/backoffice/roles')}
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
