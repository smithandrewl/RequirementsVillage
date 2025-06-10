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
	<link rel="preconnect" href="https://fonts.googleapis.com">
	<link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
	<link href="https://fonts.googleapis.com/css2?family=Cinzel:wght@400;500;600&family=Crimson+Text:wght@400;600&display=swap" rel="stylesheet">
</svelte:head>

<style>
	.gothic-bg {
		background: linear-gradient(135deg, #1a1a1a 0%, #2d2d2d 25%, #1a1a1a 50%, #2d2d2d 75%, #1a1a1a 100%);
		background-size: 400% 400%;
		animation: subtleShift 20s ease-in-out infinite;
	}
	
	@keyframes subtleShift {
		0%, 100% { background-position: 0% 50%; }
		50% { background-position: 100% 50%; }
	}
	
	.title-font {
		font-family: 'Cinzel', serif;
	}
	
	.body-font {
		font-family: 'Crimson Text', serif;
	}
	
	.gothic-shadow {
		text-shadow: 2px 2px 4px rgba(0, 0, 0, 0.8);
	}
	
	.paper-texture {
		background-image: 
			radial-gradient(circle at 20% 80%, rgba(120, 119, 198, 0.1) 0%, transparent 50%),
			radial-gradient(circle at 80% 20%, rgba(255, 255, 255, 0.05) 0%, transparent 50%),
			radial-gradient(circle at 40% 40%, rgba(120, 119, 198, 0.05) 0%, transparent 50%);
	}
</style>

<div class="min-h-screen gothic-bg paper-texture relative overflow-hidden">
	<!-- Theme selector in top right -->
	<div class="absolute top-4 right-4 z-20">
		<ThemeSelector />
	</div>
	
	<!-- Atmospheric overlay -->
	<div class="absolute inset-0 bg-gradient-to-b from-transparent via-black/10 to-black/30 z-10"></div>
	
	<!-- Main hero content -->
	<div class="hero min-h-screen relative z-10">
		<div class="hero-content text-center">
			<div class="max-w-4xl">
				{#if mounted}
					<div class="space-y-8">
						<!-- Main title -->
						<h1 class="text-6xl md:text-8xl font-semibold text-white title-font gothic-shadow tracking-wider">
							REQUIREMENTS<br />VILLAGE
						</h1>
						
						<!-- Subtitle -->
						<p class="text-xl md:text-2xl text-gray-300 body-font gothic-shadow max-w-2xl mx-auto leading-relaxed">
							Where project ideas get laid to rest
						</p>
						
						<!-- Call to action -->
						<div class="pt-8">
							<a href="/dashboard" class="btn btn-primary btn-lg px-12 py-4 text-lg font-medium tracking-wide gothic-shadow hover:scale-105 transition-transform duration-200">
								GET STARTED
							</a>
						</div>
						
						<!-- Decorative elements -->
						<div class="flex justify-center items-center space-x-4 pt-12 opacity-60">
							<div class="w-16 h-px bg-gray-500"></div>
							<div class="text-gray-500 text-sm body-font">⚰️</div>
							<div class="w-16 h-px bg-gray-500"></div>
						</div>
					</div>
				{/if}
			</div>
		</div>
	</div>
	
	<!-- Bottom fade -->
	<div class="absolute bottom-0 left-0 right-0 h-32 bg-gradient-to-t from-black/20 to-transparent z-10"></div>
</div>