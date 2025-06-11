<script lang="ts">
  import { page } from '$app/stores';

  interface NavItem {
    label: string;
    href: string;
    icon: string;
  }

  const navItems: NavItem[] = [
    { label: 'Dashboard', href: '/dashboard', icon: '📊' },
    { label: 'Projects', href: '/projects', icon: '📁' },
    { label: 'Archive', href: '/archive', icon: '📦' },
    { label: 'Settings', href: '/settings', icon: '⚙️' }
  ];

  // Access the version injected by Vite at build time
  const version = __APP_VERSION__;
</script>

<div class="w-64 min-h-screen bg-neutral text-neutral-content flex flex-col">
  <!-- Logo/Brand -->
  <div class="p-6 border-b border-neutral-focus">
    <a
      href  = "/"
      class = "text-xl font-bold text-neutral-content hover:text-white transition-colors"
    >
      Requirements<br />Village
    </a>
  </div>

  <!-- Navigation -->
  <nav class="flex-1 p-4">
    <ul class="space-y-2">
      {#each navItems as item}
        <li>
          <a
            href  = {item.href}
            class = "flex items-center space-x-3 px-4 py-3 rounded-lg transition-colors duration-200 {
              $page.url.pathname === item.href 
                ? 'bg-primary text-primary-content' 
                : 'text-neutral-content hover:bg-neutral-focus hover:text-white'
            }"
          >
            <span class="text-lg">{item.icon}</span>
            <span class="font-medium">{item.label}</span>
          </a>
        </li>
      {/each}
    </ul>
  </nav>

  <!-- Footer -->
  <div class="p-4 border-t border-neutral-focus">
    <div class="text-xs text-neutral-content/60">
      v{version}
    </div>
  </div>
</div>