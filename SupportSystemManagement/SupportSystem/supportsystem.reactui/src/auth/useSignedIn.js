import { useSelector } from "react-redux";
import { useIsAuthenticated, useMsal } from "@azure/msal-react";
import { InteractionStatus } from "@azure/msal-browser";

// Signed in with either option: a password-login session or a Microsoft Entra ID account in MSAL.
// `pending` is true while MSAL is still processing a sign-in/out redirect, so callers don't redirect too early.
export default function useSignedIn() {
  const { inProgress } = useMsal();
  const isEntraSignedIn = useIsAuthenticated();
  const passwordSignedIn = useSelector((state) => state.auth.passwordSignedIn);

  return {
    signedIn: passwordSignedIn || isEntraSignedIn,
    pending: inProgress !== InteractionStatus.None,
  };
}
