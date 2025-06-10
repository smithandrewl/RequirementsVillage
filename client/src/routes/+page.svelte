<script lang="ts">
	import { onMount } from 'svelte';
	import ThemeSelector from '$lib/components/ThemeSelector.svelte';

	let mounted = false;

	onMount(() => {
		mounted = true;
	});
</script>

<svelte:head>
	<title>Requirements Village</title>
	<meta name="description" content="Where project ideas get laid to rest" />
</svelte:head>

<div class="min-h-screen bg-base-200">
	<!-- Theme selector in top right -->
	<div class="absolute top-4 right-4 z-10">
		<ThemeSelector />
	</div>

	<div class="hero min-h-screen">
		<div class="hero-content text-center">
			<div class="max-w-md">
				<h1 class="text-5xl font-bold text-primary">Requirements Village</h1>
				<p class="py-6 text-lg text-base-content/70">Where project ideas get laid to rest</p>
				
				{#if mounted}
					<div class="card bg-base-100 shadow-xl border border-base-300">
						<div class="card-body">
							<h2 class="card-title text-primary">Gothic Theme Demo</h2>
							<p class="text-base-content">Custom theme based on UI mockups with warm browns and muted tones</p>
							<div class="flex gap-2 my-4">
								<div class="badge badge-primary">Primary</div>
								<div class="badge badge-secondary">Secondary</div>
								<div class="badge badge-accent">Accent</div>
							</div>
							<div class="card-actions justify-end">
								<button class="btn btn-secondary btn-outline">Secondary</button>
								<button class="btn btn-primary">Primary Action</button>
							</div>
						</div>
					</div>
				{/if}
			</div>
		</div>
	</div>
</div>