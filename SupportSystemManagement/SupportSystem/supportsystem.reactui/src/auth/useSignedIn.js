import { useSelector } from "react-redux";

// Signed in with either option: both end in the same Support System session (see session.js). index.js finishes
// any Microsoft sign-in redirect and restores the session before the first render, so there is no pending state.
export default function useSignedIn() {
  const signedIn = useSelector((state) => state.auth.signedIn);
  return { signedIn };
}
