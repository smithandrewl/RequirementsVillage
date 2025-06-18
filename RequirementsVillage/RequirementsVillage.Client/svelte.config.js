import adapter from '@sveltejs/adapter-static';
import { vitePreprocess } from '@sveltejs/vite-plugin-svelte';

/** @type {import('@sveltejs/kit').Config} */
const config = {
	preprocess: vitePreprocess(),
	kit: {
		adapter: adapter({
			pages: '../RequirementsVillage.Api/wwwroot',
			assets: '../RequirementsVillage.Api/wwwroot',
			fallback: 'index.html',
			precompress: false,
			strict: false
		})
	}
};

export default config;