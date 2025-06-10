<script lang="ts">
	import ThemeSelector from '$lib/components/ThemeSelector.svelte';
</script>

<svelte:head>
	<title>Dashboard - Requirements Village</title>
	<meta name="description" content="Your project ideas dashboard" />
</svelte:head>

<div class="min-h-screen bg-base-200">
	<!-- Header -->
	<div class="navbar bg-base-100 shadow-lg">
		<div class="flex-1">
			<a href="/" class="btn btn-ghost text-xl font-bold text-primary">Requirements Village</a>
		</div>
		<div class="flex-none">
			<ThemeSelector />
		</div>
	</div>

	<!-- Main content -->
	<div class="container mx-auto px-4 py-8">
		<div class="hero bg-base-100 rounded-lg shadow-xl">
			<div class="hero-content text-center py-16">
				<div class="max-w-md">
					<h1 class="text-4xl font-bold text-primary mb-4">Welcome to the Village</h1>
					<p class="text-lg text-base-content/70 mb-6">
						Your digital cemetery for project ideas is ready. Soon you'll be able to catalog, organize, and reflect on all those projects that may never see the light of day.
					</p>
					<div class="space-y-4">
						<div class="alert alert-info">
							<svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" class="stroke-current shrink-0 w-6 h-6">
								<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path>
							</svg>
							<span>Dashboard features coming soon...</span>
						</div>
						<a href="/" class="btn btn-primary">Back to Landing</a>
					</div>
				</div>
			</div>
		</div>
	</div>
</div>