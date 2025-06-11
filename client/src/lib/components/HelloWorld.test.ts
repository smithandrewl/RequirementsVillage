import { render } from '@testing-library/svelte';
import { describe, it, expect } from 'vitest';
import HelloWorld from './HelloWorld.svelte';

describe('HelloWorld', () => {
  it('renders with default name', () => {
    const { getByText } = render(HelloWorld);
    expect(getByText('Hello World!')).toBeTruthy();
  });

  it('renders with custom name', () => {
    const { getByText } = render(HelloWorld, { name: 'Requirements Village' });
    expect(getByText('Hello Requirements Village!')).toBeTruthy();
  });
});