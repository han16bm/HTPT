import { useEffect, useRef } from 'react';

/**
 * Attach the returned ref to any element whose CSS class includes
 * `scroll-animate`.  When the element first enters the viewport the hook
 * appends the `visible` class, triggering the reveal transition defined
 * in index.scss.
 */
export function useScrollAnimation<T extends HTMLElement = HTMLElement>(
  threshold = 0.12,
) {
  const ref = useRef<T>(null);

  useEffect(() => {
    const el = ref.current;
    if (!el) return;

    // Graceful fallback for very old browsers
    if (!('IntersectionObserver' in window)) {
      el.classList.add('visible');
      return;
    }

    const observer = new IntersectionObserver(
      ([entry]) => {
        if (entry.isIntersecting) {
          el.classList.add('visible');
          observer.unobserve(el);
        }
      },
      { threshold },
    );

    observer.observe(el);
    return () => observer.disconnect();
  }, [threshold]);

  return ref;
}
