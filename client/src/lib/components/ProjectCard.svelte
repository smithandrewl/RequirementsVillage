<script lang="ts">
  export let title:       string;
  export let description: string;
  export let techStack:   string[] = [];
  export let status:      'Someday' | 'Current' | 'Archive' = 'Someday';

  const statusColors = {
    'Someday': 'badge-secondary',
    'Current': 'badge-info',
    'Archive': 'badge-neutral'
  };

  function handleStatusChange(event: Event) {
    const target = event.target as HTMLSelectElement;
    status = target.value as typeof status;
  }
</script>

<div class="card bg-base-100 shadow-sm border border-base-300 hover:shadow-md transition-shadow duration-200">
  <div class="card-body p-6">
    <!-- Header with title and edit button -->
    <div class="flex justify-between items-start mb-3">
      <h3 class="card-title text-lg font-semibold text-base-content">
        {title}
      </h3>
      <button class="btn btn-ghost btn-sm btn-square opacity-60 hover:opacity-100 transition-opacity">
        <svg
          xmlns        = "http://www.w3.org/2000/svg"
          fill         = "none"
          viewBox      = "0 0 24 24"
          stroke-width = "1.5"
          stroke       = "currentColor"
          class        = "w-4 h-4"
        >
          <path
            stroke-linecap  = "round"
            stroke-linejoin = "round"
            d               = "m16.862 4.487 1.687-1.688a1.875 1.875 0 1 1 2.652 2.652L10.582 16.07a4.5 4.5 0 0 1-1.897 1.13L6 18l.8-2.685a4.5 4.5 0 0 1 1.13-1.897l8.932-8.931Zm0 0L19.5 7.125M18 14v4.75A2.25 2.25 0 0 1 15.75 21H5.25A2.25 2.25 0 0 1 3 18.75V8.25A2.25 2.25 0 0 1 5.25 6H10"
          />
        </svg>
      </button>
    </div>

    <!-- Description -->
    <p class="text-base-content/70 text-sm mb-4 leading-relaxed">
      {description}
    </p>

    <!-- Tech stack badges -->
    {#if techStack.length > 0}
      <div class="flex flex-wrap gap-2 mb-4">
        {#each techStack as tech}
          <span class="badge badge-outline badge-sm">{tech}</span>
        {/each}
      </div>
    {/if}

    <!-- Status dropdown -->
    <div class="card-actions justify-end">
      <div class="dropdown dropdown-end">
        <div
          tabindex = "0"
          role     = "button"
          class    = "btn btn-sm {statusColors[status]} gap-2"
        >
          {status}
          <svg
            xmlns        = "http://www.w3.org/2000/svg"
            fill         = "none"
            viewBox      = "0 0 24 24"
            stroke-width = "1.5"
            stroke       = "currentColor"
            class        = "w-3 h-3"
          >
            <path
              stroke-linecap  = "round"
              stroke-linejoin = "round"
              d               = "m19.5 8.25-7.5 7.5-7.5-7.5"
            />
          </svg>
        </div>
        <ul
          tabindex = "0"
          class    = "dropdown-content menu bg-base-100 rounded-box z-[1] w-32 p-2 shadow-lg border border-base-300"
        >
          <li>
            <button
              class    = "btn btn-ghost btn-sm justify-start"
              on:click = {() => status = 'Someday'}
            >
              Someday
            </button>
          </li>
          <li>
            <button
              class    = "btn btn-ghost btn-sm justify-start"
              on:click = {() => status = 'Current'}
            >
              Current
            </button>
          </li>
          <li>
            <button
              class    = "btn btn-ghost btn-sm justify-start"
              on:click = {() => status = 'Archive'}
            >
              Archive
            </button>
          </li>
        </ul>
      </div>
    </div>
  </div>
</div>
