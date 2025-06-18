/** @type {import('tailwindcss').Config} */
export default {
	content: ['./src/**/*.{html,js,svelte,ts}'],
	theme: {
		extend: {}
	},
	plugins: [require('daisyui')],
	daisyui: {
		themes: [
			false, // Hide hideous default themes with pink buttons and what not
			{
				'requirements-village': {
					'primary': '#B85450',        // Brick red from highlighted sidebar item
					'primary-focus': '#9A453F',   // Darker brick red for hover
					'primary-content': '#FFFFFF', // White text on primary

					'secondary': '#6B7280',       // Medium gray for secondary elements
					'secondary-focus': '#4B5563', // Darker gray
					'secondary-content': '#FFFFFF', // White text on secondary

					'accent': '#D1D5DB',          // Light gray accent
					'accent-focus': '#9CA3AF',    // Medium gray for accent focus
					'accent-content': '#1F2937',  // Dark text on accent

					'neutral': '#374151',         // Dark gray sidebar color from mockup
					'neutral-focus': '#1F2937',   // Darker gray
					'neutral-content': '#F9FAFB', // Almost white text on neutral

					'base-100': '#FFFFFF',        // White content areas from mockup
					'base-200': '#F3F4F6',        // Light gray page background from mockup
					'base-300': '#E5E7EB',        // Slightly darker for borders/dividers
					'base-content': '#1F2937',    // Dark gray text

					'info': '#3B82F6',            // Blue info
					'success': '#10B981',         // Green success
					'warning': '#F59E0B',         // Amber warning
					'error': '#EF4444',           // Red error
				},
				'requirements-village-dark': {
					'primary': '#DC6B67',         // Lighter brick red for dark mode
					'primary-focus': '#B85450',   // Original brick red
					'primary-content': '#FFFFFF', // White text

					'secondary': '#6B7280',       // Keep same gray tones
					'secondary-focus': '#4B5563', // Darker gray
					'secondary-content': '#FFFFFF', // White text

					'accent': '#9CA3AF',          // Medium gray accent for dark
					'accent-focus': '#6B7280',    // Darker gray
					'accent-content': '#F9FAFB',  // Light text

					'neutral': '#4B5563',         // Lighter than sidebar for contrast
					'neutral-focus': '#374151',   // Dark gray
					'neutral-content': '#F9FAFB', // Light text

					'base-100': '#1F2937',        // Dark background
					'base-200': '#111827',        // Darker page background
					'base-300': '#374151',        // Component backgrounds
					'base-content': '#F9FAFB',    // Light text

					'info': '#60A5FA',            // Blue info
					'success': '#34D399',         // Green success
					'warning': '#FBBF24',         // Amber warning
					'error': '#F87171',           // Red error
				}
			}
		]
	}
};
