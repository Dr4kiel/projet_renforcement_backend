import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { linesService } from '../../services/lines.service';
import type { LineDto } from '../../types/api';

export const LinesPage = () => {
  const [lines, setLines] = useState<LineDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadLines();
  }, []);

  const loadLines = async () => {
    try {
      setLoading(true);
      const data = await linesService.getAll();
      setLines(data);
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erreur lors du chargement');
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: number) => {
    if (!confirm('Êtes-vous sûr de vouloir supprimer cette ligne ?')) return;

    try {
      await linesService.delete(id);
      await loadLines();
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
        <h2 className="text-xl font-semibold text-gray-900">Lignes de Production</h2>
        <Link
          to="/backoffice/lines/new"
          className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
        >
          Nouvelle ligne
        </Link>
      </div>

      <div className="overflow-x-auto">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">ID</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Nom</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Équipement</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">OF En cours</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">OF Suivant</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Changement</th>
              <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase">Actions</th>
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-gray-200">
            {lines.map((line) => (
              <tr key={line.id}>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">{line.id}</td>
                <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">{line.name}</td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{line.equipmentName || '-'}</td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{line.ofEnCoursName || '-'}</td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{line.ofSuivantName || '-'}</td>
                <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                  {line.isChangement ? `${line.tempsChangement}min` : 'Non'}
                </td>
                <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium space-x-2">
                  <Link
                    to={`/backoffice/lines/${line.id}`}
                    className="text-blue-600 hover:text-blue-900"
                  >
                    Modifier
                  </Link>
                  <button
                    onClick={() => handleDelete(line.id)}
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

      {lines.length === 0 && (
        <div className="text-center py-8 text-gray-500">
          Aucune ligne trouvée
        </div>
      )}
    </div>
  );
};
