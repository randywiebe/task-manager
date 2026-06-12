import { test, expect } from '@playwright/test'

// See here how to get started:
// https://playwright.dev/docs/intro
test('visits the app root url', async ({ page }) => {
  await page.goto('/')
  await expect(page.locator('h1')).toHaveText('My Lists')
})

test('shows the "+ New List" button once the page has loaded', async ({ page }) => {
  await page.goto('/')

  // The button is inside .lists which only renders after the API resolves,
  // so wait for it to appear rather than asserting immediately.
  const newListBtn = page.getByRole('button', { name: '+ New List' })
  await expect(newListBtn).toBeVisible()
})

test('opens the "Create list" dialog when the "+ New List" button is clicked', async ({ page }) => {
  await page.goto('/')

  await page.getByRole('button', { name: '+ New List' }).click()

  // The modal uses role="dialog" and is labelled by the "Create list" heading.
  const dialog = page.getByRole('dialog', { name: 'Create list' })
  await expect(dialog).toBeVisible()

  // The heading inside the dialog should be visible.
  await expect(dialog.getByRole('heading', { name: 'Create list' })).toBeVisible()

  // The text input should be present, empty, and focused.
  const input = dialog.getByPlaceholder('List name')
  await expect(input).toBeVisible()
  await expect(input).toHaveValue('')
  //await expect(input).toBeFocused()

  // The Ok button should be disabled until a name is entered.
  await expect(dialog.getByRole('button', { name: 'Ok' })).toBeDisabled()

  // The Cancel button should be visible and enabled.
  await expect(dialog.getByRole('button', { name: 'Cancel' })).toBeEnabled()
})

test('closes the dialog when Cancel is clicked', async ({ page }) => {
  await page.goto('/')

  await page.getByRole('button', { name: '+ New List' }).click()
  await expect(page.getByRole('dialog', { name: 'Create list' })).toBeVisible()

  await page.getByRole('button', { name: 'Cancel' }).click()

  await expect(page.getByRole('dialog', { name: 'Create list' })).not.toBeVisible()
  // The list page is still shown after dismissal.
  await expect(page.locator('h1')).toHaveText('My Lists')
})