import { expect, test } from '@playwright/test';

test('index page has expected h1', async ({ page }) => {
	await page.goto('/');
	await expect(page.getByRole('heading', { name: 'Requirements Village' })).toBeVisible();
});

test('index page has expected description', async ({ page }) => {
	await page.goto('/');
	await expect(page.getByText('Where project ideas get laid to rest')).toBeVisible();
});

test('hello world card is displayed', async ({ page }) => {
	await page.goto('/');
	await expect(page.getByText('Hello World!')).toBeVisible();
	await expect(page.getByText('SvelteKit + Tailwind CSS + DaisyUI is working perfectly')).toBeVisible();
});