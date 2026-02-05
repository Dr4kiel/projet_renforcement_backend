import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { equipmentsService } from '../../services/equipments.service';
import type { EquipmentDto } from '../../types/api';

export const EquipmentsPage = () => {
  const [equipments, setEquipments] = useState<EquipmentDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadEquipments();
  }, []);

  const loadEquipments = async () => {
    try {
      setLoading(true);
      const data = await equipmentsService.getAll();
      setEquipments(data);
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erreur lors du chargement');
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: number) => {
    if (!confirm('Êtes-vous sûr de vouloir supprimer cet équipement ?')) return;

    try {
      await equipmentsService.delete(id);
      await loadEquipments();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erreur lors de la suppression');
    }
  };

  if (loading) {
    return <div className="text-center py-4">Chargement...</div>;
  }

  if (error) {
    return <div className="bg-red-50 text-red-600 p-4 rounded-md">{error}</div>;
  }

  return (
    <div className="bg-white shadow rounded-lg">
      <div className="px-6 py-4 border-b border-gray-200 flex justify-between items-center">
        <h2 className="text-xl font-semibold text-gray-900">Équipements</h2>
        <Link
          to="/backoffice/equipments/new"
          className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
        >
          Nouvel équipement
        </Link>
      </div>

      <div className="overflow-x-auto">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">ID</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Nom</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Ligne</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Tags</th>
              <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase">Actions</th>
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-gray-200">
            {equipments.map((equipment) => (
              <tr key={equipment.id}>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">{equipment.id}</td>
                <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">{equipment.name}</td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{equipment.lineName || '-'}</td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                  {equipment.tagsCount} tag(s)
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium space-x-2">
                  <Link
                    to={`/backoffice/equipments/${equipment.id}`}
                    className="text-blue-600 hover:text-blue-900"
                  >
                    Modifier
                  </Link>
                  <button
                    onClick={() => handleDelete(equipment.id)}
                    className="text-red-600 hover:text-red-900"
                  >
                    Supprimer
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {equipments.length === 0 && (
        <div className="text-center py-8 text-gray-500">
          Aucun équipement trouvé
        </div>
      )}
    </div>
  );
};
