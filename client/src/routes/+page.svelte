<script lang="ts">
	import { onMount } from 'svelte';

	let mounted = false;

	onMount(() => {
		mounted = true;
	});
</script>

<svelte:head>
	<title>Requirements Village</title>
	<meta name="description" content="Where project ideas get laid to rest" />
</svelte:head>

<div class="hero min-h-screen bg-base-200">
	<div class="hero-content text-center">
		<div class="max-w-md">
			<h1 class="text-5xl font-bold">Requirements Village</h1>
			<p class="py-6 text-lg">Where project ideas get laid to rest</p>
			
			{#if mounted}
				<div class="card bg-base-100 shadow-xl">
					<div class="card-body">
						<h2 class="card-title">Hello World!</h2>
						<p>SvelteKit + Tailwind CSS + DaisyUI is working perfectly</p>
						<div class="card-actions justify-end">
							<button class="btn btn-primary">Get Started</button>
						</div>
					</div>
				</div>
			{/if}
		</div>
	</div>
</div>