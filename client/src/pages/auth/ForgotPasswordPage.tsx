import { useState, type FormEvent } from 'react';
import { Link } from 'react-router-dom';
import authService from '../../services/authService';
import { Input } from '../../components/ui/Input';
import { Button } from '../../components/ui/Button';
import { ErrorMessage } from '../../components/ui/ErrorMessage';
import { SuccessMessage } from '../../components/ui/SuccessMessage';

export const ForgotPasswordPage = () => {
  const [login, setLogin] = useState('');
  const [error, setError] = useState('');
  const [success, setSuccess] = useState(false);
  const [isLoading, setIsLoading] = useState(false);

  const handleSubmit = async (e: FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setError('');
    setSuccess(false);

    if (!login) {
      setError('Veuillez entrer votre identifiant');
      return;
    }
    setIsLoading(true);

    try {
      await authService.forgotPassword({ identifiant: login });
      setSuccess(true);
    } catch (err) {
      setError('Une erreur est survenue. Veuillez réessayer.');
      console.error('Erreur mot de passe oublié:', err);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-gray-100 flex items-center justify-center py-12 px-4 sm:px-6 lg:px-8">
      <div className="max-w-md w-full space-y-8">
        <div>
          <h2 className="text-center text-3xl font-extrabold text-gray-900">
            Mot de passe oublié
          </h2>
          <p className="mt-2 text-center text-sm text-gray-600">
            Entrez votre identifiant pour réinitialiser votre mot de passe
          </p>
        </div>

        <div className="bg-white shadow-md rounded-lg px-8 pt-6 pb-8">
          {success ? (
            <div className="space-y-4">
              <SuccessMessage message="Demande enregistrée (simulation)" />
              <div className="bg-yellow-50 border border-yellow-200 rounded-md p-4">
                <p className="text-sm text-yellow-800">
                  <strong>En développement :</strong> L'endpoint "forgot-password"
                  n'est pas encore implémenté dans le backend. Cette fonctionnalité
                  sera disponible prochainement.
                </p>
              </div>
              <div className="text-center">
                <Link
                  to="/login"
                  className="text-sm font-medium text-blue-600 hover:text-blue-500"
                >
                  ← Retour à la connexion
                </Link>
              </div>
            </div>
          ) : (
            <form className="space-y-6" onSubmit={handleSubmit}>
              {error && <ErrorMessage message={error} />}

              <Input
                type="identifiant"
                label="Identifiant"
                value={login}
                onChange={(e) => setLogin(e.target.value)}
                placeholder="Identifiant"
                disabled={isLoading}
                required
              />

              <Button type="submit" isLoading={isLoading}>
                Envoyer le lien
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
