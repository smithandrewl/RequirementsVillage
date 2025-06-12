<script lang="ts">
  import DashboardLayout from '$lib/components/DashboardLayout.svelte';
  import ProjectCard from '$lib/components/ProjectCard.svelte';

  let selectedStatus: 'All' | 'Someday' | 'Current' | 'Archive';

  // Sample project data matching the mockup
  const projects = [
    {
      title:       'Task Manager',
      description: 'A web-based task tracking application',
      techStack:   ['ASP.NET Core', 'React'],
      status:      'Someday' as const
    },
    {
      title:       'Expense Tracker',
      description: 'An application for monitoring personal expenses',
      techStack:   ['Angular', 'Firebase'],
      status:      'Current' as const
    },
    {
      title:       'Weather App',
      description: 'A weather forecasting application',
      techStack:   ['Vue.js', 'OpenWeather'],
      status:      'Someday' as const
    },
    {
      title: 'Blog Platform',
      description: 'A simple platform for creating and managing blogs',
      techStack: ['Node.js', 'MongoDB'],
      status: 'Archive' as const
    }
  ];

	let filteredProjects = [];

  $: {
		if (selectedStatus === 'All') {
			filteredProjects = projects;
		} else {
			filteredProjects = projects.filter(project =>
				project.status === selectedStatus
			);
		}
	}
</script>

<svelte:head>
  <title>Dashboard - Requirements Village</title>
  <meta name="description" content="Your project ideas dashboard" />
</svelte:head>

<DashboardLayout>

	<select bind:value={selectedStatus}>
		<option value="All">All</option>
		<option value="Current">Current</option>
		<option value="Someday">Someday</option>
		<option value="Archive">Archived</option>
	</select>

  <!-- Projects grid -->
  {#if filteredProjects.length > 0}
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">

			{#each filteredProjects as project}
				<ProjectCard
					title={project.title}
					description={project.description}
				/>
			{/each}
    </div>
  {:else}
    <div class="text-center py-12">
      <div class="text-base-content/60 mb-4">
        <svg
          xmlns        = "http://www.w3.org/2000/svg"
          fill         = "none"
          viewBox      = "0 0 24 24"
          stroke-width = "1.5"
          stroke       = "currentColor"
          class        = "w-16 h-16 mx-auto mb-4"
        >
          <path
            stroke-linecap  = "round"
            stroke-linejoin = "round"
            d               = "M19.5 14.25v-2.625a3.375 3.375 0 0 0-3.375-3.375h-4.5V6.375a1.125 1.125 0 0 0-1.125-1.125H6.75a1.125 1.125 0 0 0-1.125 1.125v7.875B-2.25 0 0 0 4.5 16.5H13.5a1.125 1.125 0 0 0 1.125-1.125V14.25"
          />
        </svg>
      </div>
      <h3 class="text-lg font-medium text-base-content mb-2">
        No projects
      </h3>
      <p class="text-base-content/60 mb-4">
        Get started by creating your first project idea.
      </p>
      <button class="btn btn-primary">Add Project</button>
    </div>
  {/if}
</DashboardLayout>
