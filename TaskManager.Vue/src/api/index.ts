import type { TaskList } from '../models/taskList'

const BASE = import.meta.env.VITE_API_BASE_URL

async function request<T>(path: string, options?: RequestInit): Promise<T> {
  const res = await fetch(`${BASE}${path}`, options)
  if (!res.ok) throw new Error(`${res.status} ${res.statusText}`)

  // Some endpoints (e.g., TypedResults.Ok) may return a 200/204 with an empty body.
  // Calling res.json() on an empty body throws "Unexpected end of JSON input".
  const text = await res.text()
  if (!text) return undefined as unknown as T

  try {
    return JSON.parse(text) as T
  } catch (e) {
    throw new Error(e instanceof Error ? e.message : String(e))
  }
}

function json(method: string, body: unknown): RequestInit {
  return {
    method,
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
  }
}

export const api = {
  getLists:   ()                             => request<TaskList[]>('/lists'),
  getList:    (id: number)                   => request<TaskList>(`/lists/${id}`),
  createList: (summary: string)              => request<TaskList>('/lists', json('POST', { summary })),
  updateList: (id: number, summary: string)  => request<TaskList>(`/lists/${id}`, json('PUT', { summary })),
  deleteList: (id: number)                   => request<void>(`/lists/${id}`, { method: 'DELETE' }),

  // Tasks
  getTasks:   (listId: number)               => request<any[]>(`/lists/${listId}/tasks`),
  createTask: (listId: number, task: unknown) => request<any>(`/lists/${listId}/tasks`, json('POST', task)),
  updateTask: (listId: number, taskId: number, body: unknown) => request<any>(`/lists/${listId}/tasks/${taskId}`, json('PUT', body)),
}