import { test, expect } from '@playwright/test';

// 1. Reset storageState to empty for THIS file only
// This overrides the global config, so the browser starts fresh (logged out)
test.use({ storageState: { cookies: [], origins: [] } });

test('Login with username and password', async ({ page, browserName }) => {
    // Skip this test if running on WebKit because of localhost HTTP cookie restrictions
  if (browserName === 'webkit') {
    test.skip();
  }
  await page.goto('http://localhost:5173/');
  await page.getByRole('button', { name: 'Begin Session' }).click();

  // Fill the login form and submit
  await page.getByRole('textbox', { name: 'Email address' }).click();
  await page.getByRole('textbox', { name: 'Email address' }).fill('test@test.com');
  await page.getByRole('textbox', { name: 'Password' }).click();
  await page.getByRole('textbox', { name: 'Password' }).fill('SuperSecretPassword123!');
  await page.getByRole('button', { name: 'Continue', exact: true }).click();

    //Lands on the dashboard
    
  await expect(page.getByText('Dialogue with Marcus Aurelius')).toBeVisible();
  await expect(page.getByRole('heading', { name: 'What troubles you, friend?' })).toBeVisible();
});