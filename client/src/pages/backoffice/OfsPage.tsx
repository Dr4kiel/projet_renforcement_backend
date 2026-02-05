import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { ofsService } from '../../services/ofs.service';
import type { OfDto } from '../../types/api';

export const OfsPage = () => {
  const [ofs, setOfs] = useState<OfDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadOfs();
  }, []);

  const loadOfs = async () => {
    try {
      setLoading(true);
      const data = await ofsService.getAll();
      setOfs(data);
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erreur lors du chargement');
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id: number) => {
    if (!confirm('Êtes-vous sûr de vouloir supprimer cet OF ?')) return;

    try {
      await ofsService.delete(id);
      await loadOfs();
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
        <h2 className="text-xl font-semibold text-gray-900">Ordres de Fabrication (OFs)</h2>
        <Link
          to="/backoffice/ofs/new"
          className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700"
        >
          Nouvel OF
        </Link>
      </div>

      <div className="overflow-x-auto">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">ID</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">OF</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Produit</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Qté Produite</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Qté Totale</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Progression</th>
              <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase">Actions</th>
            </tr>
          </thead>
          <tbody className="bg-white divide-y divide-gray-200">
            {ofs.map((of) => {
              const progress = (of.qteProduite / of.qteTotale) * 100;
              return (
                <tr key={of.id}>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">{of.id}</td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">{of.of}</td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{of.produit}</td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{of.qteProduite}</td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{of.qteTotale}</td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                    <div className="flex items-center">
                      <div className="w-full bg-gray-200 rounded-full h-2 mr-2">
                        <div
                          className="bg-blue-600 h-2 rounded-full"
                          style={{ width: `${progress}%` }}
                        ></div>
                      </div>
                      <span>{progress.toFixed(0)}%</span>
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium space-x-2">
                    <Link
                      to={`/backoffice/ofs/${of.id}`}
                      className="text-blue-600 hover:text-blue-900"
                    >
                      Modifier
                    </Link>
                    <button
                      onClick={() => handleDelete(of.id)}
                      className="text-red-600 hover:text-red-900"
                    >
                      Supprimer
                    </button>
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>

      {ofs.length === 0 && (
        <div className="text-center py-8 text-gray-500">
          Aucun OF trouvé
        </div>
      )}
    </div>
  );
};
