<script lang="ts">
  import Sidebar from './Sidebar.svelte';
  import ThemeSelector from './ThemeSelector.svelte';

  export let title:    string = 'Requirements Village';
  export let subtitle: string = 'Where project ideas get laid to rest';
</script>

<div class="flex h-screen bg-base-200">
  <!-- Sidebar -->
  <Sidebar />

  <!-- Main content area -->
  <div class="flex-1 flex flex-col overflow-hidden">
    <!-- Header -->
    <header class="bg-base-100 border-b border-base-300 px-6 py-4">
      <div class="flex justify-between items-center">
        <div>
          <h1 class="text-2xl font-bold text-base-content">{title}</h1>
          <p class="text-sm text-base-content/60">{subtitle}</p>
        </div>
        <ThemeSelector />
      </div>
    </header>

    <!-- Content area -->
    <main class="flex-1 overflow-auto p-6">
      <slot />
    </main>
  </div>
</div>
