<script lang="ts">
  import { browser } from '$app/environment';
  import { onMount } from 'svelte';

  let currentTheme = 'requirements-village';

  const themes = [
    { value: 'requirements-village',      label: 'Light' },
    { value: 'requirements-village-dark', label: 'Dark'  },
  ];

  function setTheme(theme: string) {
    currentTheme = theme;
    if (browser) {
      document.documentElement.setAttribute('data-theme', theme);
      localStorage.setItem('theme', theme);
    }
  }

  onMount(() => {
    if (browser) {
      const savedTheme = localStorage.getItem('theme');
      if (savedTheme) {
        setTheme(savedTheme);
      }
    }
  });
</script>

<div class="dropdown dropdown-end">
  <div
    tabindex = "0"
    role     = "button"
    class    = "btn btn-ghost btn-sm"
  >
    🎨 Theme
  </div>
  <ul
    tabindex = "0"
    class    = "dropdown-content menu bg-base-100 rounded-box z-[1] w-52 p-2 shadow"
  >
    {#each themes as theme}
      <li>
        <button
          class            = "btn btn-sm btn-ghost justify-start"
          class:btn-active = { currentTheme === theme.value }
          on:click         = { () => setTheme(theme.value)  }
        >
          {theme.label}
        </button>
      </li>
    {/each}
  </ul>
</div>
