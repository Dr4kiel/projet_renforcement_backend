import { useAuth } from '../context/AuthContext';

export const DashboardPage = () => {
  const { user, logout } = useAuth();

  return (
    <div className="min-h-screen bg-gray-100">
      {/* Header */}
      <nav className="bg-white shadow-sm">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between items-center h-16">
            <div className="flex items-center">
              <h1 className="text-xl font-bold text-gray-900">
                Dashboard de Production
              </h1>
            </div>

            <div className="flex items-center space-x-4">
              <div className="text-sm">
                <p className="font-medium text-gray-900">{user?.identifiant}</p>
                <p className="text-gray-500">{user?.roleName}</p>
              </div>
              <button
                onClick={logout}
                className="px-4 py-2 text-sm font-medium text-white bg-red-600 rounded-md hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500"
              >
                Déconnexion
              </button>
            </div>
          </div>
        </div>
      </nav>

      {/* Main content */}
      <main className="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
        <div className="px-4 py-6 sm:px-0">
          <div className="bg-white shadow rounded-lg p-6">
            <h2 className="text-2xl font-bold text-gray-900 mb-4">
              Bienvenue, {user?.identifiant} !
            </h2>

            <div className="space-y-4">
              <div className="border-l-4 border-blue-500 bg-blue-50 p-4">
                <div className="flex">
                  <div className="ml-3">
                    <p className="text-sm text-blue-700">
                      <strong>Page protégée</strong> - Vous êtes maintenant
                      connecté(e) à votre tableau de bord.
                    </p>
                  </div>
                </div>
              </div>

              <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
                <div className="bg-gray-50 p-4 rounded-lg border border-gray-200">
                  <h3 className="text-sm font-medium text-gray-500">
                    Identifiant
                  </h3>
                  <p className="mt-1 text-lg font-semibold text-gray-900">
                    {user?.identifiant}
                  </p>
                </div>

                <div className="bg-gray-50 p-4 rounded-lg border border-gray-200">
                  <h3 className="text-sm font-medium text-gray-500">Email</h3>
                  <p className="mt-1 text-lg font-semibold text-gray-900">
                    {user?.email}
                  </p>
                </div>

                <div className="bg-gray-50 p-4 rounded-lg border border-gray-200">
                  <h3 className="text-sm font-medium text-gray-500">Rôle</h3>
                  <p className="mt-1 text-lg font-semibold text-gray-900">
                    {user?.roleName}
                  </p>
                </div>
              </div>

              <div className="border-t border-gray-200 pt-6 mt-6">
                <h3 className="text-lg font-semibold text-gray-900 mb-4">
                  Accès rapide au Back-Office
                </h3>
                <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-3">
                  <a
                    href="/backoffice/users"
                    className="p-4 bg-white border border-gray-200 rounded-lg hover:border-blue-500 hover:shadow-md transition-all"
                  >
                    <h4 className="font-medium text-gray-900">Utilisateurs</h4>
                    <p className="text-sm text-gray-500 mt-1">Gérer les utilisateurs</p>
                  </a>
                  <a
                    href="/backoffice/roles"
                    className="p-4 bg-white border border-gray-200 rounded-lg hover:border-blue-500 hover:shadow-md transition-all"
                  >
                    <h4 className="font-medium text-gray-900">Rôles</h4>
                    <p className="text-sm text-gray-500 mt-1">Gérer les rôles</p>
                  </a>
                  <a
                    href="/backoffice/equipments"
                    className="p-4 bg-white border border-gray-200 rounded-lg hover:border-blue-500 hover:shadow-md transition-all"
                  >
                    <h4 className="font-medium text-gray-900">Équipements</h4>
                    <p className="text-sm text-gray-500 mt-1">Gérer les équipements</p>
                  </a>
                  <a
                    href="/backoffice/lines"
                    className="p-4 bg-white border border-gray-200 rounded-lg hover:border-blue-500 hover:shadow-md transition-all"
                  >
                    <h4 className="font-medium text-gray-900">Lignes</h4>
                    <p className="text-sm text-gray-500 mt-1">Gérer les lignes de production</p>
                  </a>
                  <a
                    href="/backoffice/ofs"
                    className="p-4 bg-white border border-gray-200 rounded-lg hover:border-blue-500 hover:shadow-md transition-all"
                  >
                    <h4 className="font-medium text-gray-900">OFs</h4>
                    <p className="text-sm text-gray-500 mt-1">Gérer les ordres de fabrication</p>
                  </a>
                  <a
                    href="/backoffice/tags"
                    className="p-4 bg-white border border-gray-200 rounded-lg hover:border-blue-500 hover:shadow-md transition-all"
                  >
                    <h4 className="font-medium text-gray-900">Tags</h4>
                    <p className="text-sm text-gray-500 mt-1">Gérer les tags de capteurs</p>
                  </a>
                </div>
              </div>
            </div>
          </div>
        </div>
      </main>
    </div>
  );
};
