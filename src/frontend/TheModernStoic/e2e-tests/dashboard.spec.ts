import { test, expect } from "@playwright/test";

test("Submit a user entry", async ({ page }) => {
  await page.goto("http://localhost:5173/");

  // Create a unique string using the current timestamp
  const uniqueEntry = `I'm anxious about my future - ${Date.now()}`;

  await page.getByRole("button", { name: "Begin Session" }).click();

  //Auth0 prompts for authorization
  await page.getByRole("button", { name: "Accept" }).click();

  //Should have already been logged in via setup

  //Write and submit entry
  await page.getByPlaceholder("Write your thoughts here...").click();
  await page.getByPlaceholder("Write your thoughts here...").fill(uniqueEntry);
  await page.getByRole("button", { name: "Reflect" }).click();

  //Make sure the response overlay appears
  await expect(
    page
      .getByRole("paragraph")
      .filter({ hasText: "The Stoic is contemplating..." })
  ).toBeVisible();

  await page.waitForSelector("text=Close", {
    state: "visible",
    timeout: 10000,
  });
  await expect(page.getByRole("button", { name: "Close" })).toBeVisible();
  await page.getByRole("button", { name: "Close" }).click();

  //Check if the entry is in the history tab
  await page.getByRole("button", { name: "History" }).click();
  await expect(page.getByText(uniqueEntry)).toBeVisible();
});
