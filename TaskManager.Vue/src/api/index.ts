const BASE = import.meta.env.VITE_API_BASE_URL

async function request<T>(path: string, options?: RequestInit): Promise<T> {
  const res = await fetch(`${BASE}${path}`, options)
  if (!res.ok) throw new Error(`${res.status} ${res.statusText}`)
  return res.json()
}

function json(method: string, body: unknown): RequestInit {
  return {
    method,
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  }
}

export interface TaskList {
  id: number
  summary: string
}

export const api = {
  getLists:   ()                             => request<TaskList[]>('/lists'),
  getList:    (id: number)                   => request<TaskList>(`/lists/${id}`),
  createList: (summary: string)              => request<TaskList>('/lists', json('POST', { summary })),
  updateList: (id: number, summary: string)  => request<TaskList>(`/lists/${id}`, json('PUT', { summary })),
  deleteList: (id: number)                   => request<void>(`/lists/${id}`, { method: 'DELETE' }),
}