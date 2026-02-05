import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { usersService } from '../../services/users.service';
import { rolesService } from '../../services/roles.service';
import type { CreateUserRequestDto, UpdateUserRequestDto, RoleDto } from '../../types/api';

export const UserFormPage = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const isEdit = !!id;

  const [formData, setFormData] = useState({
    identifiant: '',
    email: '',
    password: '',
    roleId: '' as string | number,
  });
  const [roles, setRoles] = useState<RoleDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadRoles();
    if (isEdit) {
      loadUser();
    }
  }, [id]);

  const loadRoles = async () => {
    try {
      const data = await rolesService.getAll();
      setRoles(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erreur lors du chargement des rôles');
    }
  };

  const loadUser = async () => {
    try {
      const user = await usersService.getById(Number(id));
      setFormData({
        identifiant: user.identifiant,
        email: user.email,
        password: '',
        roleId: user.roleId || '',
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
        const updateData: UpdateUserRequestDto = {
          identifiant: formData.identifiant,
          email: formData.email,
          roleId: formData.roleId ? Number(formData.roleId) : null,
        };
        await usersService.update(Number(id), updateData);
      } else {
        const createData: CreateUserRequestDto = {
          identifiant: formData.identifiant,
          email: formData.email,
          password: formData.password,
          roleId: formData.roleId ? Number(formData.roleId) : null,
        };
        await usersService.create(createData);
      }
      navigate('/backoffice/users');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erreur lors de la sauvegarde');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="bg-white shadow rounded-lg p-6">
      <h2 className="text-2xl font-bold text-gray-900 mb-6">
        {isEdit ? 'Modifier l\'utilisateur' : 'Nouvel utilisateur'}
      </h2>

      {error && (
        <div className="bg-red-50 text-red-600 p-4 rounded-md mb-4">{error}</div>
      )}

      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="block text-sm font-medium text-gray-700">Identifiant</label>
          <input
            type="text"
            required
            value={formData.identifiant}
            onChange={(e) => setFormData({ ...formData, identifiant: e.target.value })}
            className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 px-3 py-2 border"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700">Email</label>
          <input
            type="email"
            required
            value={formData.email}
            onChange={(e) => setFormData({ ...formData, email: e.target.value })}
            className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 px-3 py-2 border"
          />
        </div>

        {!isEdit && (
          <div>
            <label className="block text-sm font-medium text-gray-700">Mot de passe</label>
            <input
              type="password"
              required={!isEdit}
              value={formData.password}
              onChange={(e) => setFormData({ ...formData, password: e.target.value })}
              className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 px-3 py-2 border"
              minLength={6}
            />
          </div>
        )}

        <div>
          <label className="block text-sm font-medium text-gray-700">Rôle</label>
          <select
            value={formData.roleId}
            onChange={(e) => setFormData({ ...formData, roleId: e.target.value })}
            className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 px-3 py-2 border"
          >
            <option value="">Aucun rôle</option>
            {roles.map((role) => (
              <option key={role.id} value={role.id}>
                {role.name}
              </option>
            ))}
          </select>
        </div>

        <div className="flex justify-end space-x-3 pt-4">
          <button
            type="button"
            onClick={() => navigate('/backoffice/users')}
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
