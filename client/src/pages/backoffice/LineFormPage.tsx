import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { linesService } from '../../services/lines.service';
import { equipmentsService } from '../../services/equipments.service';
import { ofsService } from '../../services/ofs.service';
import type { CreateLineRequestDto, UpdateLineRequestDto, EquipmentDto, OfDto } from '../../types/api';

export const LineFormPage = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const isEdit = !!id;

  const [formData, setFormData] = useState({
    name: '',
    isChangement: false,
    tempsChangement: 0,
    equipmentId: '' as string | number,
    ofEnCoursId: '' as string | number,
    ofSuivantId: '' as string | number,
  });
  const [equipments, setEquipments] = useState<EquipmentDto[]>([]);
  const [ofs, setOfs] = useState<OfDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadData();
    if (isEdit) {
      loadLine();
    }
  }, [id]);

  const loadData = async () => {
    try {
      const [equipmentsData, ofsData] = await Promise.all([
        equipmentsService.getAll(),
        ofsService.getAll(),
      ]);
      setEquipments(equipmentsData);
      setOfs(ofsData);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erreur lors du chargement des données');
    }
  };

  const loadLine = async () => {
    try {
      const line = await linesService.getById(Number(id));
      setFormData({
        name: line.name,
        isChangement: line.isChangement,
        tempsChangement: line.tempsChangement,
        equipmentId: line.equipmentId,
        ofEnCoursId: line.ofEnCoursId || '',
        ofSuivantId: line.ofSuivantId || '',
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
        const updateData: UpdateLineRequestDto = {
          name: formData.name,
          isChangement: formData.isChangement,
          tempsChangement: Number(formData.tempsChangement),
          equipmentId: Number(formData.equipmentId),
          ofEnCoursId: formData.ofEnCoursId ? Number(formData.ofEnCoursId) : null,
          ofSuivantId: formData.ofSuivantId ? Number(formData.ofSuivantId) : null,
        };
        await linesService.update(Number(id), updateData);
      } else {
        const createData: CreateLineRequestDto = {
          name: formData.name,
          isChangement: formData.isChangement,
          tempsChangement: Number(formData.tempsChangement),
          equipmentId: Number(formData.equipmentId),
          ofEnCoursId: formData.ofEnCoursId ? Number(formData.ofEnCoursId) : undefined,
          ofSuivantId: formData.ofSuivantId ? Number(formData.ofSuivantId) : undefined,
        };
        await linesService.create(createData);
      }
      navigate('/backoffice/lines');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erreur lors de la sauvegarde');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="bg-white shadow rounded-lg p-6">
      <h2 className="text-2xl font-bold text-gray-900 mb-6">
        {isEdit ? 'Modifier la ligne' : 'Nouvelle ligne'}
      </h2>

      {error && (
        <div className="bg-red-50 text-red-600 p-4 rounded-md mb-4">{error}</div>
      )}

      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="block text-sm font-medium text-gray-700">Nom de la ligne</label>
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
          <label className="block text-sm font-medium text-gray-700">Équipement</label>
          <select
            required
            value={formData.equipmentId}
            onChange={(e) => setFormData({ ...formData, equipmentId: e.target.value })}
            className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 px-3 py-2 border"
          >
            <option value="">Sélectionner un équipement</option>
            {equipments.map((equipment) => (
              <option key={equipment.id} value={equipment.id}>
                {equipment.name}
              </option>
            ))}
          </select>
        </div>

        <div>
          <label className="flex items-center">
            <input
              type="checkbox"
              checked={formData.isChangement}
              onChange={(e) => setFormData({ ...formData, isChangement: e.target.checked })}
              className="h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
            />
            <span className="ml-2 text-sm font-medium text-gray-700">En changement</span>
          </label>
        </div>

        {formData.isChangement && (
          <div>
            <label className="block text-sm font-medium text-gray-700">Temps de changement (minutes)</label>
            <input
              type="number"
              min="0"
              value={formData.tempsChangement}
              onChange={(e) => setFormData({ ...formData, tempsChangement: Number(e.target.value) })}
              className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 px-3 py-2 border"
            />
          </div>
        )}

        <div>
          <label className="block text-sm font-medium text-gray-700">OF en cours</label>
          <select
            value={formData.ofEnCoursId}
            onChange={(e) => setFormData({ ...formData, ofEnCoursId: e.target.value })}
            className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 px-3 py-2 border"
          >
            <option value="">Aucun OF</option>
            {ofs.map((of) => (
              <option key={of.id} value={of.id}>
                {of.of} - {of.produit}
              </option>
            ))}
          </select>
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700">OF suivant</label>
          <select
            value={formData.ofSuivantId}
            onChange={(e) => setFormData({ ...formData, ofSuivantId: e.target.value })}
            className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 px-3 py-2 border"
          >
            <option value="">Aucun OF</option>
            {ofs.map((of) => (
              <option key={of.id} value={of.id}>
                {of.of} - {of.produit}
              </option>
            ))}
          </select>
        </div>

        <div className="flex justify-end space-x-3 pt-4">
          <button
            type="button"
            onClick={() => navigate('/backoffice/lines')}
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
