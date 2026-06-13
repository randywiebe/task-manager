<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { api } from '../api'

interface Task {
  id: number
  summary: string
  complete: boolean
}

const route = useRoute()
const router = useRouter()
const listId = Number(route.params.id || 0)

const tasks = ref<Task[]>([])
const loading = ref(true)
const error = ref<string | null>(null)

// Dialog state
const showDialog = ref(false)
const editingTask = ref<Task | null>(null)
const editSummary = ref('')
const saving = ref(false)

async function fetchTasks() {
  loading.value = true
  error.value = null
  try {
    const res = await api.getTasks(listId)
    // assume API returns array of tasks with id, summary, complete
    tasks.value = res.map((t: any) => ({ id: t.id, summary: t.summary, complete: !!t.complete }))
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Something went wrong'
  } finally {
    loading.value = false
  }
}

function openEdit(task: Task) {
  editingTask.value = task
  editSummary.value = task.summary
  showDialog.value = true
}

function closeDialog() {
  showDialog.value = false
  editingTask.value = null
  editSummary.value = ''
  saving.value = false
}

async function confirmEdit() {
  if (!editingTask.value) return
  saving.value = true
  try {
    await api.updateTask(listId, editingTask.value.id, { summary: editSummary.value })
    // update local copy
    const t = tasks.value.find(x => x.id === editingTask.value!.id)
    if (t) t.summary = editSummary.value
    closeDialog()
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Something went wrong'
  } finally {
    saving.value = false
  }
}

async function toggleComplete(task: Task) {
  // Optimistic UI: flip locally first
  const prev = task.complete
  task.complete = !task.complete
  try {
    await api.updateTask(listId, task.id, { summary: task.summary, complete: task.complete })
  } catch (e) {
    // revert on error
    task.complete = prev
    error.value = e instanceof Error ? e.message : 'Something went wrong'
  }
}

function goBack() {
  router.push('/')
}

onMounted(fetchTasks)
</script>

<template>
  <main class="page">
    <header class="page-header">
      <h1 class="page-title">Tasks</h1>
      <p class="page-subtitle">Tasks for list {{ listId }}</p>
    </header>

    <div v-if="loading" class="state-container" aria-live="polite">
      <span class="spinner" aria-hidden="true"></span>
      <p class="state-label">Fetching tasks…</p>
    </div>

    <div v-else-if="error" class="state-container error-state" aria-live="assertive">
      <p class="state-label">{{ error }}</p>
      <button class="retry-btn" @click="fetchTasks">Try again</button>
    </div>

    <ul v-else class="lists" role="list">
      <li v-for="task in tasks" :key="task.id" class="list-item">
        <div class="list-card" style="display:flex; align-items:center; gap:12px;">
          <input type="checkbox" :checked="task.complete" @change="() => toggleComplete(task)" aria-label="Complete task" />
          <button class="list-card" style="flex:1; text-align:left; background:transparent; border:none; padding:0;" @click="openEdit(task)">
            <input type="text" :value="task.summary" readonly style="width:100%; border:none; background:transparent; padding:0; font-size:1rem;" />
          </button>
        </div>
      </li>
    </ul>

    <div style="margin-top:1rem;">
      <button class="btn" @click="goBack">Back</button>
    </div>

    <!-- Edit dialog -->
    <teleport to="body">
      <div v-if="showDialog" class="modal-backdrop" role="dialog" aria-modal="true">
        <div class="modal">
          <h2>Edit summary</h2>
          <label class="label">
            <input type="text" v-model="editSummary" />
          </label>
          <div class="modal-actions">
            <button class="btn" @click="confirmEdit" :disabled="saving">Ok</button>
            <button class="btn secondary" @click="closeDialog">Cancel</button>
          </div>
        </div>
      </div>
    </teleport>

  </main>
</template>

<style scoped>
/* reuse some styles from AllLists for consistency */
.page { max-width: 640px; margin: 0 auto; padding: 3rem 1.5rem 4rem; }
.page-header { margin-bottom: 2.5rem; }
.page-title { font-size: 2rem; font-weight: 700; margin: 0 0 0.375rem; }
.page-subtitle { font-size: 0.9375rem; color: var(--color-text); opacity:0.6 }
.lists { list-style:none; padding:0; margin:0; display:flex; flex-direction:column; gap:0.625rem }
.list-item { }
.list-card { display:flex; align-items:center; justify-content:space-between; width:100%; padding:1rem 1.25rem; background:var(--color-background-soft); border:1px solid var(--color-border); border-radius:10px; }
.label input { width:100%; padding:0.5rem; border:1px solid var(--color-border); border-radius:6px; }
.modal-backdrop { position: fixed; inset: 0; display:flex; align-items:center; justify-content:center; background: rgba(0,0,0,0.35); z-index:1000 }
.modal { background:#fff; padding:1.25rem; border-radius:8px; width:90%; max-width:420px; box-shadow: 0 6px 20px rgba(0,0,0,0.12); }
.modal-actions { display:flex; gap:0.5rem; justify-content:flex-end; margin-top:1rem }
.btn { padding:0.5rem 0.9rem; border-radius:6px; border:1px solid var(--color-border); background:var(--color-background-soft); cursor:pointer }
.btn.secondary { background:transparent }
.retry-btn { margin-top:0.25rem; padding:0.5rem 1.25rem }
.spinner { display:block; width:2rem; height:2rem; border:2.5px solid var(--color-border); border-top-color:var(--color-text); border-radius:50%; animation:spin 0.75s linear infinite }
@keyframes spin { to { transform: rotate(360deg); } }
</style>
