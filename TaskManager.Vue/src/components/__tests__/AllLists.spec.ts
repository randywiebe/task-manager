import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import AllLists from '../AllLists.vue'

// ── Mocks ────────────────────────────────────────────────────────────────────

const mockPush = vi.fn()

// vi.hoisted runs before vi.mock factories, making these available inside
// the factory without a static '../api' import that vue-tsc cannot resolve.
const { mockGetLists, mockCreateList } = vi.hoisted(() => ({
  mockGetLists: vi.fn(),
  mockCreateList: vi.fn(),
}))

vi.mock('vue-router', () => ({
  useRouter: () => ({ push: mockPush }),
}))

vi.mock('../../api', () => ({
  api: {
    getLists: mockGetLists,
    createList: mockCreateList,
  },
}))

// ── Helpers ───────────────────────────────────────────────────────────────────

function mountAllLists() {
  return mount(AllLists, { global: { stubs: { teleport: true } } })
}

/** Mounts the component, waits for the initial fetch, then opens the dialog. */
async function mountWithDialogOpen() {
  mockGetLists.mockResolvedValue([])
  const wrapper = mountAllLists()
  await flushPromises()
  await wrapper.find('.list-card').trigger('click') // first card is always "+ New List"
  return wrapper
}

// ── Tests ─────────────────────────────────────────────────────────────────────

describe('AllLists.vue', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  // ── Loading ────────────────────────────────────────────────────────────────

  describe('loading state', () => {
    it('shows the spinner before the API resolves', () => {
      mockGetLists.mockReturnValue(new Promise(() => {})) // never resolves

      const wrapper = mountAllLists()

      expect(wrapper.find('[aria-label="Loading lists"]').exists()).toBe(true)
      expect(wrapper.find('.spinner').exists()).toBe(true)
    })

    it('hides the spinner once the API resolves', async () => {
      mockGetLists.mockResolvedValue([])

      const wrapper = mountAllLists()
      await flushPromises()

      expect(wrapper.find('.spinner').exists()).toBe(false)
    })
  })

  // ── Error ──────────────────────────────────────────────────────────────────

  describe('error state', () => {
    it('shows the error message when the API rejects', async () => {
      mockGetLists.mockRejectedValue(new Error('500 Internal Server Error'))

      const wrapper = mountAllLists()
      await flushPromises()

      expect(wrapper.find('.error-state').exists()).toBe(true)
      expect(wrapper.find('.state-label').text()).toBe('500 Internal Server Error')
    })

    it('shows a fallback message for non-Error rejections', async () => {
      mockGetLists.mockRejectedValue('network failure')

      const wrapper = mountAllLists()
      await flushPromises()

      expect(wrapper.find('.state-label').text()).toBe('Something went wrong')
    })

    it('retries the API call when "Try again" is clicked', async () => {
      mockGetLists
        .mockRejectedValueOnce(new Error('500 Internal Server Error'))
        .mockResolvedValueOnce([])

      const wrapper = mountAllLists()
      await flushPromises()

      await wrapper.find('.retry-btn').trigger('click')
      await flushPromises()

      expect(mockGetLists).toHaveBeenCalledTimes(2)
      expect(wrapper.find('.error-state').exists()).toBe(false)
    })
  })

  // ── Empty state ────────────────────────────────────────────────────────────

  describe('empty state', () => {
    it('shows only the "+ New List" card when the API returns no lists', async () => {
      // NOTE: The component always renders .lists (it contains the "+ New List"
      // button). The empty state is identified by a single card, not by the
      // absence of .lists. There is no separate empty-state label in this design.
      mockGetLists.mockResolvedValue([])

      const wrapper = mountAllLists()
      await flushPromises()

      expect(wrapper.find('.lists').exists()).toBe(true)
      expect(wrapper.findAll('.list-card')).toHaveLength(1)
      expect(wrapper.find('.list-summary').text()).toBe('+ New List')
    })
  })

  // ── Populated state ────────────────────────────────────────────────────────

  describe('populated state', () => {
    const fakeLists = [
      { id: 1, summary: 'Groceries' },
      { id: 2, summary: 'Weekend errands' },
      { id: 3, summary: 'Home repairs' },
    ]

    it('renders a card for each list plus the "+ New List" card', async () => {
      mockGetLists.mockResolvedValue(fakeLists)

      const wrapper = mountAllLists()
      await flushPromises()

      // +1 for the always-present "+ New List" card
      expect(wrapper.findAll('.list-card')).toHaveLength(fakeLists.length + 1)
    })

    it('displays the correct summary on each list card', async () => {
      mockGetLists.mockResolvedValue(fakeLists)

      const wrapper = mountAllLists()
      await flushPromises()

      // First card is "+ New List"; skip it
      const summaries = wrapper.findAll('.list-summary').slice(1).map(el => el.text())
      expect(summaries).toEqual(fakeLists.map(l => l.summary))
    })

    it('navigates to the correct route when a list card is clicked', async () => {
      mockGetLists.mockResolvedValue(fakeLists)

      const wrapper = mountAllLists()
      await flushPromises()

      // cards[0] is "+ New List"; cards[1] is "Groceries" (id: 1), cards[2] is "Weekend errands" (id: 2)
      const cards = wrapper.findAll('.list-card')
      await cards[2]!.trigger('click')

      expect(mockPush).toHaveBeenCalledOnce()
      expect(mockPush).toHaveBeenCalledWith('/lists/2')
    })
  })

  // ── Create-list dialog ─────────────────────────────────────────────────────

  describe('create-list dialog', () => {
    describe('opening and closing', () => {
      it('is hidden on initial render', async () => {
        mockGetLists.mockResolvedValue([])
        const wrapper = mountAllLists()
        await flushPromises()

        expect(wrapper.find('.modal-backdrop').exists()).toBe(false)
      })

      it('opens when the "+ New List" card is clicked', async () => {
        const wrapper = await mountWithDialogOpen()

        expect(wrapper.find('.modal-backdrop').exists()).toBe(true)
        expect(wrapper.find('h2').text()).toBe('Create list')
      })

      it('closes when the Cancel button is clicked', async () => {
        const wrapper = await mountWithDialogOpen()

        await wrapper.find('.btn.secondary').trigger('click')

        expect(wrapper.find('.modal-backdrop').exists()).toBe(false)
      })

      it('resets the input field when the dialog is reopened', async () => {
        mockGetLists.mockResolvedValue([])
        const wrapper = mountAllLists()
        await flushPromises()

        // Open → type → cancel → reopen
        await wrapper.find('.list-card').trigger('click')
        await wrapper.find('input').setValue('half-typed name')
        await wrapper.find('.btn.secondary').trigger('click')
        await wrapper.find('.list-card').trigger('click')

        expect((wrapper.find('input').element as HTMLInputElement).value).toBe('')
      })
    })

    describe('input validation', () => {
      it('disables the Ok button when the input is empty', async () => {
        const wrapper = await mountWithDialogOpen()
        const okBtn = wrapper.find('.btn:not(.secondary)')

        expect((okBtn.element as HTMLButtonElement).disabled).toBe(true)
      })

      it('enables the Ok button when the input has valid text', async () => {
        const wrapper = await mountWithDialogOpen()
        await wrapper.find('input').setValue('My new list')
        const okBtn = wrapper.find('.btn:not(.secondary)')

        expect((okBtn.element as HTMLButtonElement).disabled).toBe(false)
      })

      it('disables the Ok button and shows an error for summaries over 50 characters', async () => {
        const wrapper = await mountWithDialogOpen()
        await wrapper.find('input').setValue('A'.repeat(51))

        const okBtn = wrapper.find('.btn:not(.secondary)')
        expect((okBtn.element as HTMLButtonElement).disabled).toBe(true)
        expect(wrapper.find('.validation-error').text()).toBe(
          'Summary must be 50 characters or fewer',
        )
      })

      it('shows no validation error for a summary of exactly 50 characters', async () => {
        const wrapper = await mountWithDialogOpen()
        await wrapper.find('input').setValue('A'.repeat(50))

        expect(wrapper.find('.validation-error').exists()).toBe(false)
        expect((wrapper.find('.btn:not(.secondary)').element as HTMLButtonElement).disabled).toBe(false)
      })

      it('disables the Ok button for a whitespace-only summary', async () => {
        const wrapper = await mountWithDialogOpen()
        await wrapper.find('input').setValue('   ')
        const okBtn = wrapper.find('.btn:not(.secondary)')

        expect((okBtn.element as HTMLButtonElement).disabled).toBe(true)
      })
    })

    describe('successful creation', () => {
      it('calls api.createList with the entered summary', async () => {
        mockCreateList.mockResolvedValue(undefined)
        mockGetLists.mockResolvedValue([]) // second call after creation

        const wrapper = await mountWithDialogOpen()
        await wrapper.find('input').setValue('Groceries')
        await wrapper.find('.btn:not(.secondary)').trigger('click')
        await flushPromises()

        expect(mockCreateList).toHaveBeenCalledOnce()
        expect(mockCreateList).toHaveBeenCalledWith('Groceries')
      })

      it('re-fetches the list after successful creation', async () => {
        mockCreateList.mockResolvedValue(undefined)
        mockGetLists.mockResolvedValue([])

        const wrapper = await mountWithDialogOpen()
        await wrapper.find('input').setValue('Groceries')
        await wrapper.find('.btn:not(.secondary)').trigger('click')
        await flushPromises()

        // First call on mount, second call after successful create
        expect(mockGetLists).toHaveBeenCalledTimes(2)
      })

      it('closes the dialog after successful creation', async () => {
        mockCreateList.mockResolvedValue(undefined)
        mockGetLists.mockResolvedValue([])

        const wrapper = await mountWithDialogOpen()
        await wrapper.find('input').setValue('Groceries')
        await wrapper.find('.btn:not(.secondary)').trigger('click')
        await flushPromises()

        expect(wrapper.find('.modal-backdrop').exists()).toBe(false)
      })

      it('shows the new list after creation', async () => {
        mockCreateList.mockResolvedValue(undefined)
        mockGetLists
          .mockResolvedValueOnce([])
          .mockResolvedValueOnce([{ id: 42, summary: 'Groceries' }])

        const wrapper = await mountWithDialogOpen()
        await wrapper.find('input').setValue('Groceries')
        await wrapper.find('.btn:not(.secondary)').trigger('click')
        await flushPromises()

        const summaries = wrapper.findAll('.list-summary').map(el => el.text())
        expect(summaries).toContain('Groceries')
      })
    })

    describe('failed creation', () => {
      it('shows the error message when api.createList rejects', async () => {
        mockCreateList.mockRejectedValue(new Error('Failed to create'))
        mockGetLists.mockResolvedValue([])

        const wrapper = await mountWithDialogOpen()
        await wrapper.find('input').setValue('Groceries')
        await wrapper.find('.btn:not(.secondary)').trigger('click')
        await flushPromises()

        expect(wrapper.find('.error-state').exists()).toBe(true)
        expect(wrapper.find('.state-label').text()).toBe('Failed to create')
      })

      it('shows a fallback error for non-Error rejections from createList', async () => {
        mockCreateList.mockRejectedValue('timeout')
        mockGetLists.mockResolvedValue([])

        const wrapper = await mountWithDialogOpen()
        await wrapper.find('input').setValue('Groceries')
        await wrapper.find('.btn:not(.secondary)').trigger('click')
        await flushPromises()

        expect(wrapper.find('.state-label').text()).toBe('Something went wrong')
      })

      it('leaves the dialog open so the user can retry', async () => {
        mockCreateList.mockRejectedValue(new Error('Failed to create'))
        mockGetLists.mockResolvedValue([])

        const wrapper = await mountWithDialogOpen()
        await wrapper.find('input').setValue('Groceries')
        await wrapper.find('.btn:not(.secondary)').trigger('click')
        await flushPromises()

        // Dialog stays open; user can correct and try again
        expect(wrapper.find('.modal-backdrop').exists()).toBe(true)
      })

      it('does not re-fetch the list when creation fails', async () => {
        mockCreateList.mockRejectedValue(new Error('Failed to create'))
        mockGetLists.mockResolvedValue([])

        const wrapper = await mountWithDialogOpen()
        await wrapper.find('input').setValue('Groceries')
        await wrapper.find('.btn:not(.secondary)').trigger('click')
        await flushPromises()

        // Only the initial mount fetch should have been called
        expect(mockGetLists).toHaveBeenCalledTimes(1)
      })
    })
  })
})