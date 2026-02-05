import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import { ProtectedRoute } from './components/auth/ProtectedRoute';
import { LoginPage } from './pages/auth/LoginPage';
import { DashboardPage } from './pages/DashboardPage';
import { BackOfficeLayout } from './components/layout/BackOfficeLayout';
import { UsersPage } from './pages/backoffice/UsersPage';
import { UserFormPage } from './pages/backoffice/UserFormPage';
import { RolesPage } from './pages/backoffice/RolesPage';
import { RoleFormPage } from './pages/backoffice/RoleFormPage';
import { EquipmentsPage } from './pages/backoffice/EquipmentsPage';
import { EquipmentFormPage } from './pages/backoffice/EquipmentFormPage';
import { LinesPage } from './pages/backoffice/LinesPage';
import { LineFormPage } from './pages/backoffice/LineFormPage';
import { OfsPage } from './pages/backoffice/OfsPage';
import { OfFormPage } from './pages/backoffice/OfFormPage';
import { TagsPage } from './pages/backoffice/TagsPage';
import { TagFormPage } from './pages/backoffice/TagFormPage';
import { ProductionDashboardPage } from './pages/ProductionDashboardPage';

function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          {/* Routes publiques */}
          <Route path="/login" element={<LoginPage />} />

          {/* Routes protégées */}
          <Route
            path="/dashboard"
            element={
              <ProtectedRoute>
                <ProductionDashboardPage />
              </ProtectedRoute>
            }
          />

          {/* Dashboard simple (pour admin ou debugging) */}
          <Route
            path="/dashboard/simple"
            element={
              <ProtectedRoute>
                <DashboardPage />
              </ProtectedRoute>
            }
          />

          {/* Routes du back-office */}
          <Route
            path="/backoffice"
            element={
              <ProtectedRoute>
                <BackOfficeLayout />
              </ProtectedRoute>
            }
          >
            {/* Redirect /backoffice to /backoffice/users by default */}
            <Route index element={<Navigate to="users" replace />} />

            {/* Users */}
            <Route path="users" element={<UsersPage />} />
            <Route path="users/new" element={<UserFormPage />} />
            <Route path="users/:id" element={<UserFormPage />} />

            {/* Roles */}
            <Route path="roles" element={<RolesPage />} />
            <Route path="roles/new" element={<RoleFormPage />} />
            <Route path="roles/:id" element={<RoleFormPage />} />

            {/* Equipments */}
            <Route path="equipments" element={<EquipmentsPage />} />
            <Route path="equipments/new" element={<EquipmentFormPage />} />
            <Route path="equipments/:id" element={<EquipmentFormPage />} />

            {/* Lines */}
            <Route path="lines" element={<LinesPage />} />
            <Route path="lines/new" element={<LineFormPage />} />
            <Route path="lines/:id" element={<LineFormPage />} />

            {/* OFs */}
            <Route path="ofs" element={<OfsPage />} />
            <Route path="ofs/new" element={<OfFormPage />} />
            <Route path="ofs/:id" element={<OfFormPage />} />

            {/* Tags */}
            <Route path="tags" element={<TagsPage />} />
            <Route path="tags/new" element={<TagFormPage />} />
            <Route path="tags/:id" element={<TagFormPage />} />
          </Route>

          {/* Redirections */}
          <Route path="/" element={<Navigate to="/dashboard" replace />} />
          <Route path="*" element={<Navigate to="/login" replace />} />
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  );
}

export default App;
