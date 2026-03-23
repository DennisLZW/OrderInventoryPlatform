type Props = {
  title?: string
  message: string
}

export function ErrorState({
  title = 'Request failed',
  message,
}: Props) {
  return (
    <div className="admin-fetch-state admin-fetch-state--error" role="alert">
      <strong>{title}</strong>
      <p>{message}</p>
    </div>
  )
}
