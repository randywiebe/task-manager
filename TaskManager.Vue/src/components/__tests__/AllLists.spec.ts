import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import AllLists from '../AllLists.vue'

// ── Mocks ────────────────────────────────────────────────────────────────────

const mockPush = vi.fn()

// vi.hoisted runs before vi.mock factories, making mockGetLists available inside
// the factory without a static '../api' import that vue-tsc cannot resolve.
const mockGetLists = vi.hoisted(() => vi.fn())

vi.mock('vue-router', () => ({
  useRouter: () => ({ push: mockPush }),
}))

vi.mock('../api', () => ({
  api: {
    getLists: mockGetLists,
  },
}))

// ── Helpers ───────────────────────────────────────────────────────────────────

function mountAllLists() {
  return mount(AllLists, { global: { stubs: { teleport: true } } })
}

// ── Tests ─────────────────────────────────────────────────────────────────────

describe('AllLists.vue', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  describe('loading state', () => {
    it('shows the spinner before the API resolves', () => {
      // Never resolves during this test
      mockGetLists.mockReturnValue(new Promise(() => {}))

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

  describe('empty state', () => {
    it('shows the empty message when the API returns no lists', async () => {
      mockGetLists.mockResolvedValue([])

      const wrapper = mountAllLists()
      await flushPromises()

      expect(wrapper.find('.state-label').text()).toBe('No lists yet. Create one to get started.')
      expect(wrapper.find('.lists').exists()).toBe(false)
    })
  })

  describe('populated state', () => {
    const fakeLists = [
      { id: 1, summary: 'Groceries' },
      { id: 2, summary: 'Weekend errands' },
      { id: 3, summary: 'Home repairs' },
    ]

    it('renders a card for each list', async () => {
      mockGetLists.mockResolvedValue(fakeLists)

      const wrapper = mountAllLists()
      await flushPromises()

      const cards = wrapper.findAll('.list-card')
      expect(cards).toHaveLength(fakeLists.length)
    })

    it('displays the correct summary on each card', async () => {
      mockGetLists.mockResolvedValue(fakeLists)

      const wrapper = mountAllLists()
      await flushPromises()

      const summaries = wrapper.findAll('.list-summary').map(el => el.text())
      expect(summaries).toEqual(fakeLists.map(l => l.summary))
    })

    it('navigates to the correct route when a card is clicked', async () => {
      mockGetLists.mockResolvedValue(fakeLists)

      const wrapper = mountAllLists()
      await flushPromises()

      const cards = wrapper.findAll('.list-card')
      const secondCard = cards[1]
      expect(secondCard).toBeDefined()
      await secondCard!.trigger('click')

      expect(mockPush).toHaveBeenCalledOnce()
      expect(mockPush).toHaveBeenCalledWith('/lists/2')
    })
  })
})