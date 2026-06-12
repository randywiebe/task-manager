<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import { api } from '../api'
import { type TaskList } from '../models/taskList'

const router = useRouter()
const lists = ref<TaskList[]>([])
const loading = ref(true)
const error = ref<string | null>(null)

// Dialog / create state
const showDialog = ref(false)
const newSummary = ref('')
const creating = ref(false)
const validationError = computed(() => newSummary.value.length > 50 ? 'Summary must be 50 characters or fewer' : null)

async function fetchLists() {
  loading.value = true
  error.value = null
  try {
    lists.value = await api.getLists()
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Something went wrong'
  } finally {
    loading.value = false
  }
}

function goToList(id: number) {
  router.push(`/lists/${id}`)
}

function openCreateDialog() {
  newSummary.value = ''
  showDialog.value = true
}

function closeDialog() {
  showDialog.value = false
  creating.value = false
  newSummary.value = ''
}

async function confirmCreate() {
  if (newSummary.value.length > 50) return
  creating.value = true
  try {
    // Set the TaskList summary and POST to /lists
    await api.createList(newSummary.value)
    await fetchLists()
    closeDialog()
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Something went wrong'
  } finally {
    creating.value = false
  }
}

onMounted(fetchLists)
</script>

<template>
  <main class="page">
    <header class="page-header">
      <h1 class="page-title">My Lists</h1>
      <p class="page-subtitle">Pick up where you left off.</p>
    </header>

    <!-- Loading -->
    <div v-if="loading" class="state-container" aria-live="polite" aria-label="Loading lists">
      <span class="spinner" aria-hidden="true"></span>
      <p class="state-label">Fetching your lists…</p>
    </div>

    <!-- Error -->
    <div v-else-if="error" class="state-container error-state" aria-live="assertive">
      <svg class="error-icon" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none"
        stroke="currentColor" stroke-width="1.75" stroke-linecap="round" stroke-linejoin="round"
        aria-hidden="true">
        <circle cx="12" cy="12" r="10" />
        <line x1="12" y1="8" x2="12" y2="12" />
        <line x1="12" y1="16" x2="12.01" y2="16" />
      </svg>
      <p class="state-label">{{ error }}</p>
      <button class="retry-btn" @click="fetchLists">Try again</button>
    </div>

    <!-- List (create entry first) -->
    <ul v-else class="lists" role="list">
      <li class="list-item">
        <button class="list-card" @click="openCreateDialog" type="button">
          <span class="list-summary">+ New List</span>
          <svg class="list-arrow" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none"
            stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"
            aria-hidden="true">
          </svg>
        </button>
      </li>
      <li v-for="list in lists" :key="list.id" class="list-item">
        <button class="list-card" @click="goToList(list.id)">
          <span class="list-summary">{{ list.summary }}</span>
          <svg class="list-arrow" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none"
            stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"
            aria-hidden="true">
            <line x1="5" y1="12" x2="19" y2="12" />
            <polyline points="12 5 19 12 12 19" />
          </svg>
        </button>
      </li>
    </ul>

    <!-- Create dialog -->
    <teleport to="body">
      <div v-if="showDialog" class="modal-backdrop" role="dialog" aria-modal="true" aria-labelledby="create-list-title">
        <div class="modal">
          <h2 id="create-list-title">Create list</h2>
          <label class="label">
            <input type="text" v-model="newSummary" placeholder="List name" />
          </label>
          <p v-if="validationError" class="validation-error">{{ validationError }}</p>
          <div class="modal-actions">
            <button class="btn" @click="confirmCreate" :disabled="!!validationError || creating || newSummary.trim() === ''">Ok</button>
            <button class="btn secondary" @click="closeDialog">Cancel</button>
          </div>
        </div>
      </div>
    </teleport>

  </main>
</template>

<style scoped>
/* ── Layout ── */
.page {
  max-width: 640px;
  margin: 0 auto;
  padding: 3rem 1.5rem 4rem;
}

/* ── Header ── */
.page-header {
  margin-bottom: 2.5rem;
}

.page-title {
  font-size: 2rem;
  font-weight: 700;
  letter-spacing: -0.03em;
  line-height: 1.1;
  color: var(--color-heading);
  margin: 0 0 0.375rem;
}

.page-subtitle {
  font-size: 0.9375rem;
  color: var(--color-text);
  opacity: 0.6;
  margin: 0;
}

/* ── State containers (loading / error / empty) ── */
.state-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.875rem;
  padding: 4rem 1rem;
  text-align: center;
}

.state-label {
  font-size: 0.9375rem;
  color: var(--color-text);
  opacity: 0.65;
  margin: 0;
}

/* ── Spinner ── */
.spinner {
  display: block;
  width: 2rem;
  height: 2rem;
  border: 2.5px solid var(--color-border);
  border-top-color: var(--color-text);
  border-radius: 50%;
  animation: spin 0.75s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

@media (prefers-reduced-motion: reduce) {
  .spinner {
    animation: none;
    opacity: 0.4;
  }
}

/* ── Error state ── */
.error-state {
  color: #c0392b;
}

.error-icon {
  width: 2rem;
  height: 2rem;
  color: #c0392b;
}

.error-state .state-label {
  color: #c0392b;
  opacity: 1;
}

.retry-btn {
  margin-top: 0.25rem;
  padding: 0.5rem 1.25rem;
  font-size: 0.875rem;
  font-weight: 500;
  background: transparent;
  border: 1.5px solid currentColor;
  border-radius: 6px;
  color: #c0392b;
  cursor: pointer;
  transition: background 0.15s, color 0.15s;
}

.retry-btn:hover {
  background: #c0392b;
  color: #fff;
}

.retry-btn:focus-visible {
  outline: 2px solid #c0392b;
  outline-offset: 2px;
}

/* ── List ── */
.lists {
  list-style: none;
  padding: 0;
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: 0.625rem;
}

/* ── Card ── */
.list-card {
  display: flex;
  align-items: center;
  justify-content: space-between;
  width: 100%;
  padding: 1rem 1.25rem;
  background: var(--color-background-soft);
  border: 1px solid var(--color-border);
  border-radius: 10px;
  cursor: pointer;
  text-align: left;
  transition: border-color 0.15s, background 0.15s, transform 0.1s;
}

.list-card:hover {
  border-color: var(--color-text);
  background: var(--color-background-mute);
}

.list-card:active {
  transform: scale(0.99);
}

.list-card:focus-visible {
  outline: 2px solid var(--color-text);
  outline-offset: 2px;
}

.list-summary {
  font-size: 1rem;
  font-weight: 500;
  color: var(--color-heading);
  line-height: 1.4;
}

.list-arrow {
  flex-shrink: 0;
  width: 1.125rem;
  height: 1.125rem;
  color: var(--color-text);
  opacity: 0.4;
  margin-left: 1rem;
  transition: opacity 0.15s, transform 0.15s;
}

.list-card:hover .list-arrow {
  opacity: 0.9;
  transform: translateX(3px);
}


/* Modal */
.modal-backdrop {
  position: fixed;
  inset: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgba(0,0,0,0.35);
  z-index: 1000;
}

.modal {
  background: #fff;
  padding: 1.25rem;
  border-radius: 8px;
  width: 90%;
  max-width: 420px;
  box-shadow: 0 6px 20px rgba(0,0,0,0.12);
}

.modal h2 {
  margin: 0 0 0.5rem;
  font-size: 1.125rem;
}

.label input {
  width: 100%;
  padding: 0.5rem;
  border: 1px solid var(--color-border);
  border-radius: 6px;
}

.validation-error {
  color: #c0392b;
  margin: 0.5rem 0 0;
  font-size: 0.875rem;
}

.modal-actions {
  display: flex;
  gap: 0.5rem;
  justify-content: flex-end;
  margin-top: 1rem;
}

.btn {
  padding: 0.5rem 0.9rem;
  border-radius: 6px;
  border: 1px solid var(--color-border);
  background: var(--color-background-soft);
  cursor: pointer;
}

.btn.secondary {
  background: transparent;
}

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}
</style>