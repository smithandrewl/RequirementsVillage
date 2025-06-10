<script lang="ts">
	import DashboardLayout from '$lib/components/DashboardLayout.svelte';
</script>

<svelte:head>
	<title>Projects - Requirements Village</title>
	<meta name="description" content="Manage your project ideas" />
</svelte:head>

<DashboardLayout title="Projects" subtitle="Manage all your project ideas">
	<div class="text-center py-12">
		<div class="text-base-content/60 mb-4">
			<svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="w-16 h-16 mx-auto mb-4">
				<path stroke-linecap="round" stroke-linejoin="round" d="M19.5 14.25v-2.625a3.375 3.375 0 0 0-3.375-3.375h-4.5V6.375a1.125 1.125 0 0 0-1.125-1.125H6.75a1.125 1.125 0 0 0-1.125 1.125v7.875b2.25 0 0 0 4.5 16.5H13.5a1.125 1.125 0 0 0 1.125-1.125V14.25" />
			</svg>
		</div>
		<h3 class="text-lg font-medium text-base-content mb-2">Projects Management</h3>
		<p class="text-base-content/60 mb-4">Advanced project management features coming soon.</p>
		<div class="space-x-4">
			<a href="/dashboard" class="btn btn-secondary">Back to Dashboard</a>
			<button class="btn btn-primary">Add Project</button>
		</div>
	</div>
</DashboardLayout>