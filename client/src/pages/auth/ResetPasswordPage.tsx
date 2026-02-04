import { useState, type FormEvent, useEffect } from 'react';
import { Link, useSearchParams, useNavigate } from 'react-router-dom';
import authService from '../../services/authService';
import { Input } from '../../components/ui/Input';
import { Button } from '../../components/ui/Button';
import { ErrorMessage } from '../../components/ui/ErrorMessage';
import { SuccessMessage } from '../../components/ui/SuccessMessage';

export const ResetPasswordPage = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();

  const [token, setToken] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [error, setError] = useState('');
  const [success, setSuccess] = useState(false);
  const [isLoading, setIsLoading] = useState(false);

  // Récupérer le token depuis l'URL au montage
  useEffect(() => {
    const tokenFromUrl = searchParams.get('token');
    if (tokenFromUrl) {
      setToken(tokenFromUrl);
    }
  }, [searchParams]);

  const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setError('');
    setSuccess(false);

    // Validation
    if (!token) {
      setError('Token manquant. Veuillez entrer votre token de réinitialisation.');
      return;
    }

    if (!password || !confirmPassword) {
      setError('Veuillez remplir tous les champs');
      return;
    }

    if (password.length < 8) {
      setError('Le mot de passe doit contenir au moins 8 caractères');
      return;
    }

    if (password !== confirmPassword) {
      setError('Les mots de passe ne correspondent pas');
      return;
    }

    setIsLoading(true);

    try {
      // TODO: Endpoint à implémenter côté backend
      await authService.resetPassword({ token, password, confirmPassword });
      setSuccess(true);

      // Rediriger vers login après 2 secondes
      setTimeout(() => {
        navigate('/login');
      }, 2000);
    } catch (err) {
      setError('Fonctionnalité non encore implémentée.');
      console.error('Erreur réinitialisation:', err);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-gray-100 flex items-center justify-center py-12 px-4 sm:px-6 lg:px-8">
      <div className="max-w-md w-full space-y-8">
        <div>
          <h2 className="text-center text-3xl font-extrabold text-gray-900">
            Réinitialiser le mot de passe
          </h2>
          <p className="mt-2 text-center text-sm text-gray-600">
            Entrez votre nouveau mot de passe
          </p>
        </div>

        <div className="bg-white shadow-md rounded-lg px-8 pt-6 pb-8">
          {success ? (
            <div className="space-y-4">
              <SuccessMessage message="Mot de passe réinitialisé avec succès !" />
              <p className="text-sm text-center text-gray-600">
                Redirection vers la page de connexion...
              </p>
            </div>
          ) : (
            <form className="space-y-6" onSubmit={handleSubmit}>
              {error && <ErrorMessage message={error} />}

              <Input
                type="text"
                label="Token de réinitialisation"
                value={token}
                onChange={(e) => setToken(e.target.value)}
                placeholder="Collez votre token ici"
                disabled={isLoading}
                required
              />

              <div className="bg-yellow-50 border border-yellow-200 rounded-md p-3">
                <p className="text-xs text-yellow-800">
                  <strong>En développement :</strong> L'endpoint "reset-password"
                  n'est pas encore implémenté dans le backend.
                </p>
              </div>

              <Input
                type="password"
                label="Nouveau mot de passe"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                placeholder="••••••••"
                disabled={isLoading}
                required
              />

              <Input
                type="password"
                label="Confirmer le mot de passe"
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                placeholder="••••••••"
                disabled={isLoading}
                required
              />

              {password && (
                <div className="text-xs text-gray-600">
                  <p className={password.length >= 8 ? 'text-green-600' : ''}>
                    ✓ Minimum 8 caractères
                  </p>
                  <p
                    className={
                      password === confirmPassword && confirmPassword
                        ? 'text-green-600'
                        : ''
                    }
                  >
                    ✓ Les mots de passe correspondent
                  </p>
                </div>
              )}

              <Button type="submit" isLoading={isLoading}>
                Réinitialiser
              </Button>
            </form>
          )}

          <div className="mt-6 text-center">
            <Link
              to="/login"
              className="text-sm font-medium text-blue-600 hover:text-blue-500"
            >
              ← Retour à la connexion
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
};
