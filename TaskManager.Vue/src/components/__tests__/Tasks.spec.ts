import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'

// Hoist mock factories so vi.mock can reference them when hoisted
const mockPush = vi.fn()
const { mockGetTasks, mockCreateTask, mockUpdateTask } = vi.hoisted(() => ({
  mockGetTasks: vi.fn(),
  mockCreateTask: vi.fn(),
  mockUpdateTask: vi.fn(),
}))

vi.mock('vue-router', () => ({
  useRoute: () => ({ params: { id: '123' } }),
  useRouter: () => ({ push: mockPush }),
}))

vi.mock('../../api', () => ({
  api: {
    getTasks: mockGetTasks,
    createTask: mockCreateTask,
    updateTask: mockUpdateTask,
  },
}))

import Tasks from '../Tasks.vue'

function mountTasks() {
  return mount(Tasks, { global: { stubs: { teleport: true } } })
}

describe('Tasks.vue', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('renders New Task button and opens the create dialog', async () => {
    mockGetTasks.mockResolvedValue([])
    const wrapper = mountTasks()
    await flushPromises()

    // Find the New Task card button specifically
    const newTaskBtn = wrapper.findAll('button').find(b => b.text().includes('New Task'))
    expect(newTaskBtn).toBeDefined()

    await newTaskBtn!.trigger('click')
    // dialog is rendered via teleport stub
    expect(wrapper.find('.modal-backdrop').exists()).toBe(true)
    expect(wrapper.find('h2').text()).toBe('New Task')
  })

  it('closes create dialog when Cancel is clicked', async () => {
    mockGetTasks.mockResolvedValue([])
    const wrapper = mountTasks()
    await flushPromises()

    const newTaskBtn = wrapper.findAll('button').find(b => b.text().includes('New Task'))
    await newTaskBtn!.trigger('click')
    await wrapper.find('.btn.secondary').trigger('click')

    expect(wrapper.find('.modal-backdrop').exists()).toBe(false)
  })

  it('shows validation error for summaries over 50 characters', async () => {
    mockGetTasks.mockResolvedValue([])
    const wrapper = mountTasks()
    await flushPromises()

    const newTaskBtn = wrapper.findAll('button').find(b => b.text().includes('New Task'))
    await newTaskBtn!.trigger('click')
    const input = wrapper.find('input')
    await input.setValue('A'.repeat(51))

    await wrapper.find('.btn:not(.secondary)').trigger('click')
    await flushPromises()

    expect(wrapper.find('.validation-error').text()).toBe('Summary must not exceed 50 characters')
  })

  it('calls api.createTask when Ok is clicked with valid summary', async () => {
    mockGetTasks.mockResolvedValue([])
    mockCreateTask.mockResolvedValue({ id: 9, summary: 'Test', complete: false })
    const wrapper = mountTasks()
    await flushPromises()

    const newTaskBtn = wrapper.findAll('button').find(b => b.text().includes('New Task'))
    await newTaskBtn!.trigger('click')
    const input = wrapper.find('input')
    await input.setValue('Test')

    await wrapper.find('.btn:not(.secondary)').trigger('click')
    await flushPromises()

    expect(mockCreateTask).toHaveBeenCalledOnce()
    expect(mockCreateTask).toHaveBeenCalledWith(123, { summary: 'Test', complete: false })
  })
})