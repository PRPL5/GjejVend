type AccessTokenProvider = () => string | null | Promise<string | null>

let provider: AccessTokenProvider = () => null

/**
 * The auth feature (Supabase sign-in) registers how to get the current access token here.
 * Until then requests are sent without a token and the API answers 401/403 for protected routes.
 */
export function setAccessTokenProvider(next: AccessTokenProvider): void {
  provider = next
}

export async function getAccessToken(): Promise<string | null> {
  return provider()
}
