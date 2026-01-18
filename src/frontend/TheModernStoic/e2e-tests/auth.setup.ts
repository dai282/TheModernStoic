import { test as setup, expect } from '@playwright/test';

const authFile = 'playwright/.auth/user.json';

setup('authenticate', async ({ page }) => {
  await page.goto('http://localhost:5173/');
  await page.getByRole('button', { name: 'Begin Session' }).click();

  // Perform the actual Auth0 login steps here
  await page.getByLabel('Email address').fill('test@test.com'); // Use env variables!
  await page.getByRole('textbox', { name: 'Password' }).fill('SuperSecretPassword123!');
  await page.getByRole('button', { name: 'Continue', exact: true }).click();

  // Wait until the page actually signs in (look for dashboard text)
  await expect(page.getByText('Dialogue with Marcus Aurelius')).toBeVisible();

  // Save the cookies and storage state to a file
  await page.context().storageState({ path: authFile });
});