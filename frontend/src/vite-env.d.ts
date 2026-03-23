/// <reference types="vite/client" />

interface ImportMetaEnv {
  /** e.g. `http://localhost:5241` — omit to use Vite `/api` proxy in dev */
  readonly VITE_API_BASE_URL?: string
  /** Legacy alias for `VITE_API_BASE_URL` */
  readonly VITE_API_URL?: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
