type Props = {
  message?: string
}

export function LoadingState({ message = 'Loading…' }: Props) {
  return (
    <p className="admin-fetch-state admin-fetch-state--loading" role="status">
      {message}
    </p>
  )
}
