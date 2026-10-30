// Landing.razor.js - GSAP 3 + ScrollTrigger Orchestration for Socratic Landing

let landingTimeline = null;
let scrollTriggers = [];
let cardTiltListeners = [];
let parallaxTweens = [];
let cursorGlowListener = null;

export async function initLandingAnimations() {
    try {
        // 1. Ensure GSAP is available (CDN fallback if not preloaded)
        if (!window.gsap) {
            await loadScript('https://cdn.jsdelivr.net/npm/gsap@3.12.5/dist/gsap.min.js');
        }
        if (!window.ScrollTrigger && window.gsap) {
            await loadScript('https://cdn.jsdelivr.net/npm/gsap@3.12.5/dist/ScrollTrigger.min.js');
        }

        const gsap = window.gsap;
        if (!gsap) {
            console.warn('[Socratic Landing] GSAP could not be loaded; standard CSS transitions active.');
            return;
        }

        if (window.ScrollTrigger) {
            gsap.registerPlugin(window.ScrollTrigger);
        }

        const prefersReducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
        if (prefersReducedMotion) {
            console.log('[Socratic Landing] Prefers-reduced-motion active: subtle animations only.');
            return;
        }

        // Clean previous instances before initializing
        disposeLandingAnimations();

        // 2. Global Atmospheric Cursor Glow Orb & Spotlight Tracking
        initCursorGlowFollower(gsap);

        // 3. Hero Stagger Entrance Animation
        landingTimeline = gsap.timeline({
            defaults: { ease: 'power3.out', duration: 0.85 }
        });

        landingTimeline
            .fromTo('.hero-badge', { opacity: 0, y: -20, scale: 0.95 }, { opacity: 1, y: 0, scale: 1, duration: 0.7, delay: 0.05, clearProps: 'transform,opacity' })
            .fromTo('.hero-title', { opacity: 0, y: 30 }, { opacity: 1, y: 0, duration: 0.9, clearProps: 'transform,opacity' }, '-=0.45')
            .fromTo('.hero-subtitle', { opacity: 0, y: 20 }, { opacity: 1, y: 0, duration: 0.75, clearProps: 'transform,opacity' }, '-=0.6')
            .fromTo('.hero-actions-primary .cta-btn, .hero-module-pills', { opacity: 0, y: 20, scale: 0.96 }, { opacity: 1, y: 0, scale: 1, stagger: 0.1, duration: 0.7, clearProps: 'all' }, '-=0.55')
            .fromTo('.hero-stage-card', { opacity: 0, scale: 0.93 }, { opacity: 1, scale: 1, duration: 1.05, ease: 'back.out(1.15)', clearProps: 'transform,opacity' }, '-=0.8')
            .fromTo('.specs-bar', { opacity: 0, y: 25 }, { opacity: 1, y: 0, duration: 0.85, clearProps: 'transform,opacity' }, '-=0.55')
            .fromTo('.specs-bar .spec-item', { opacity: 0, y: 15 }, { opacity: 1, y: 0, stagger: 0.08, duration: 0.6, clearProps: 'transform,opacity' }, '-=0.5');

        // 3. Number Counter Animation for Specs Bar
        initSpecCounters(gsap);

        // 4. Parallax Depth on Aurora Orbs & Floating Chips
        if (window.ScrollTrigger) {
            initParallaxDepth(gsap);
        }

        // 5. ScrollTrigger Reveals for Sections
        if (window.ScrollTrigger) {
            initScrollReveals(gsap);
        }

        // 6. Interactive 3D Card Tilt with spring physics
        initInteractive3DTilt(gsap);

        console.log('[Socratic Landing] GSAP animations and 3D tilt successfully initialized.');
    } catch (err) {
        console.error('[Socratic Landing] Animation initialization error:', err);
    }
}

// Parallax Depth for Floating Media & Background Ambient Spotlights
function initParallaxDepth(gsap) {
    const isDesktop = window.matchMedia('(min-width: 1024px) and (hover: hover)').matches;
    if (!isDesktop) return;

    // Parallax on Floating Chips in Hero
    const chip1 = document.querySelector('.chip-1');
    if (chip1) {
        const pt = gsap.to(chip1, {
            y: -30,
            ease: 'none',
            scrollTrigger: {
                trigger: '.hero-section',
                start: 'top top',
                end: 'bottom top',
                scrub: 1.2
            }
        });
        parallaxTweens.push(pt);
    }

    const chip2 = document.querySelector('.chip-2');
    if (chip2) {
        const pt = gsap.to(chip2, {
            y: 30,
            ease: 'none',
            scrollTrigger: {
                trigger: '.hero-section',
                start: 'top top',
                end: 'bottom top',
                scrub: 1.2
            }
        });
        parallaxTweens.push(pt);
    }

    // Parallax on Background Spotlights
    const spotHero = document.querySelector('.bg-spotlight-hero');
    if (spotHero) {
        const pt = gsap.to(spotHero, {
            y: 90,
            ease: 'none',
            scrollTrigger: {
                trigger: '.landing-container',
                start: 'top top',
                end: 'bottom bottom',
                scrub: 2
            }
        });
        parallaxTweens.push(pt);
    }

    const spotBento = document.querySelector('.bg-spotlight-bento');
    if (spotBento) {
        const pt = gsap.to(spotBento, {
            y: -70,
            ease: 'none',
            scrollTrigger: {
                trigger: '.landing-container',
                start: 'top top',
                end: 'bottom bottom',
                scrub: 2
            }
        });
        parallaxTweens.push(pt);
    }
}

// ScrollTrigger Reveals for Core Sections
function initScrollReveals(gsap) {
    // Section Title Boxes reveal
    document.querySelectorAll('.section-title-box').forEach(titleBox => {
        const st = ScrollTrigger.create({
            trigger: titleBox,
            start: 'top 88%',
            onEnter: () => {
                gsap.from(titleBox.children, {
                    opacity: 0,
                    y: 28,
                    stagger: 0.12,
                    duration: 0.75,
                    ease: 'power2.out',
                    clearProps: 'transform,opacity'
                });
            },
            once: true
        });
        scrollTriggers.push(st);
    });

    // Evolution Banner
    const evoSection = document.querySelector('.evolution-banner');
    if (evoSection) {
        const st = ScrollTrigger.create({
            trigger: evoSection,
            start: 'top 85%',
            onEnter: () => {
                gsap.from(evoSection.querySelectorAll('.evolution-header, .evolution-card, .evolution-arrow'), {
                    opacity: 0,
                    y: 35,
                    stagger: 0.14,
                    duration: 0.8,
                    ease: 'power2.out',
                    clearProps: 'transform,opacity'
                });
            },
            once: true
        });
        scrollTriggers.push(st);
    }

    // Dual Org Live Showcases Reveal
    const orgGrid = document.querySelector('.org-showcase-grid');
    if (orgGrid) {
        const st = ScrollTrigger.create({
            trigger: orgGrid,
            start: 'top 85%',
            onEnter: () => {
                gsap.from(orgGrid.querySelectorAll('.org-card'), {
                    opacity: 0,
                    y: 40,
                    scale: 0.96,
                    stagger: 0.15,
                    duration: 0.8,
                    ease: 'power2.out',
                    clearProps: 'transform,opacity'
                });
            },
            once: true
        });
        scrollTriggers.push(st);
    }

    // Spatial Technology Wings
    const spatialWings = document.querySelector('.spatial-wings-grid');
    if (spatialWings) {
        const st = ScrollTrigger.create({
            trigger: spatialWings,
            start: 'top 85%',
            onEnter: () => {
                gsap.from(spatialWings.querySelectorAll('.spatial-wing-card'), {
                    opacity: 0,
                    y: 40,
                    stagger: 0.15,
                    duration: 0.8,
                    ease: 'power2.out',
                    clearProps: 'transform,opacity'
                });
            },
            once: true
        });
        scrollTriggers.push(st);
    }

    // UX Modes Spotlight Showcase (The 4 Flagship Archetypes)
    const spotlightGrid = document.querySelector('.ux-spotlight-grid');
    if (spotlightGrid) {
        const st = ScrollTrigger.create({
            trigger: spotlightGrid,
            start: 'top 85%',
            onEnter: () => {
                gsap.from(spotlightGrid.querySelectorAll('.ux-spotlight-card'), {
                    opacity: 0,
                    y: 45,
                    scale: 0.94,
                    stagger: 0.12,
                    duration: 0.85,
                    ease: 'power3.out',
                    clearProps: 'transform,opacity'
                });
            },
            once: true
        });
        scrollTriggers.push(st);
    }

    // UX Modes Explorer Grid batch reveal
    const modesGrid = document.querySelector('.ux-modes-grid');
    if (modesGrid) {
        const st = ScrollTrigger.create({
            trigger: modesGrid,
            start: 'top 88%',
            onEnter: () => {
                gsap.from(modesGrid.querySelectorAll('.ux-mode-card'), {
                    opacity: 0,
                    y: 30,
                    stagger: 0.05,
                    duration: 0.6,
                    ease: 'power2.out',
                    clearProps: 'transform,opacity'
                });
            },
            once: true
        });
        scrollTriggers.push(st);
    }

    // Bento Superpowers reveals
    const bentoGrid = document.querySelector('.superpowers-bento-grid');
    if (bentoGrid) {
        const st = ScrollTrigger.create({
            trigger: bentoGrid,
            start: 'top 85%',
            onEnter: () => {
                gsap.from(bentoGrid.querySelectorAll('.bento-card'), {
                    opacity: 0,
                    y: 40,
                    stagger: 0.15,
                    duration: 0.8,
                    ease: 'power2.out',
                    clearProps: 'transform,opacity'
                });
            },
            once: true
        });
        scrollTriggers.push(st);
    }

    // B2B Lead Generation Banner
    const b2bSection = document.querySelector('.b2b-lead-section');
    if (b2bSection) {
        const st = ScrollTrigger.create({
            trigger: b2bSection,
            start: 'top 85%',
            onEnter: () => {
                gsap.from(b2bSection.querySelectorAll('.b2b-lead-content > *'), {
                    opacity: 0,
                    y: 30,
                    stagger: 0.12,
                    duration: 0.75,
                    ease: 'power2.out',
                    clearProps: 'transform,opacity'
                });
            },
            once: true
        });
        scrollTriggers.push(st);
    }

    // Platform Grid
    const platformGrid = document.querySelector('.platforms-unified-grid');
    if (platformGrid) {
        const st = ScrollTrigger.create({
            trigger: platformGrid,
            start: 'top 85%',
            onEnter: () => {
                gsap.from(platformGrid.querySelectorAll('.platform-card'), {
                    opacity: 0,
                    y: 35,
                    stagger: 0.1,
                    duration: 0.7,
                    ease: 'power2.out',
                    clearProps: 'transform,opacity'
                });
            },
            once: true
        });
        scrollTriggers.push(st);
    }
}

// Global Atmospheric Cursor Glow Orb & Dynamic Spotlight
function initCursorGlowFollower(gsap) {
    const isFinePointer = window.matchMedia('(pointer: fine)').matches;
    if (!isFinePointer) return;

    const orb = document.getElementById('cursorGlowOrb');
    if (!orb) return;

    // High performance 60-120fps hardware-accelerated transforms
    const setX = gsap.quickTo(orb, 'x', { duration: 0.35, ease: 'power2.out' });
    const setY = gsap.quickTo(orb, 'y', { duration: 0.35, ease: 'power2.out' });

    let isVisible = false;

    const onPointerMove = (e) => {
        if (!isVisible) {
            orb.style.opacity = '1';
            isVisible = true;
        }
        setX(e.clientX);
        setY(e.clientY);

        // Instant spotlight tracking on any hovered glass or interactive card
        const card = e.target && e.target.closest && e.target.closest(
            '.glass, .hero-card, .ux-mode-card, .org-card, .bento-card, .spatial-wing-card, .spatial-subcard, .industry-card, .evolution-card, .platform-card, .feature-card'
        );
        if (card) {
            const rect = card.getBoundingClientRect();
            card.style.setProperty('--mouse-x', `${e.clientX - rect.left}px`);
            card.style.setProperty('--mouse-y', `${e.clientY - rect.top}px`);
        }
    };

    const onPointerLeave = (e) => {
        if (!e.relatedTarget && !e.toElement) {
            orb.style.opacity = '0';
            isVisible = false;
        }
    };

    window.addEventListener('pointermove', onPointerMove, { passive: true });
    document.addEventListener('mouseleave', onPointerLeave);

    cursorGlowListener = { onPointerMove, onPointerLeave };
}

// Interactive 3D Card Tilt with spring physics (Flagship Hero and Bento cards only)
function initInteractive3DTilt(gsap) {
    const isFinePointer = window.matchMedia('(pointer: fine) and (min-width: 1024px)').matches;
    if (!isFinePointer) return;

    // Only apply 3D physics to large hero and showcase cards to avoid DOM thrashing
    const cards = document.querySelectorAll('.hero-card, .hero-stage-card, .ux-spotlight-card, .bento-card, .org-card');

    cards.forEach(card => {
        // QuickTo setters for high-performance 60-120fps hardware-accelerated transforms
        const setRotateX = gsap.quickTo(card, 'rotationX', { duration: 0.35, ease: 'power2.out' });
        const setRotateY = gsap.quickTo(card, 'rotationY', { duration: 0.35, ease: 'power2.out' });
        const setZ = gsap.quickTo(card, 'z', { duration: 0.35, ease: 'power2.out' });

        gsap.set(card, { transformPerspective: 1000, transformStyle: 'preserve-3d' });

        const onPointerMove = (e) => {
            const rect = card.getBoundingClientRect();
            const x = e.clientX - rect.left;
            const y = e.clientY - rect.top;

            // Normalized coordinates (-1 to +1)
            const normX = (x - rect.width / 2) / (rect.width / 2);
            const normY = (y - rect.height / 2) / (rect.height / 2);

            // Subtle, premium tilt angle (max 5 degrees)
            setRotateY(normX * 5.0);
            setRotateX(-normY * 5.0);
            setZ(10);

            // Radial spotlight cursor tracking
            card.style.setProperty('--mouse-x', `${x}px`);
            card.style.setProperty('--mouse-y', `${y}px`);
        };

        const onPointerLeave = () => {
            gsap.to(card, {
                rotationX: 0,
                rotationY: 0,
                z: 0,
                duration: 0.65,
                ease: 'power2.out',
                clearProps: 'rotationX,rotationY,z,transformPerspective,transformStyle'
            });
        };

        card.addEventListener('pointermove', onPointerMove, { passive: true });
        card.addEventListener('pointerleave', onPointerLeave, { passive: true });

        cardTiltListeners.push({ card, onPointerMove, onPointerLeave });
    });
}

// Switch Hero device tab mockup smoothly with GSAP
export function switchHeroDeviceTab(tab) {
    const gsap = window.gsap;
    const stageImg = document.querySelector('.hero-stage-img');
    if (!gsap || !stageImg) return;

    gsap.fromTo(stageImg,
        { opacity: 0.3, scale: 0.96, filter: 'blur(6px)' },
        { opacity: 1, scale: 1, filter: 'blur(0px)', duration: 0.4, ease: 'power2.out', clearProps: 'filter' }
    );
}

// Refresh listeners and scrollTrigger when Blazor updates DOM on category or search filter
export function refreshUxModeCards() {
    const gsap = window.gsap;
    if (!gsap) return;

    if (window.ScrollTrigger) {
        ScrollTrigger.refresh();
    }

    const cards = document.querySelectorAll('.ux-mode-card');
    if (cards.length > 0) {
        gsap.fromTo(cards, 
            { opacity: 0, y: 16, scale: 0.98 },
            { opacity: 1, y: 0, scale: 1, duration: 0.3, stagger: 0.02, ease: 'power2.out', clearProps: 'transform,opacity' }
        );
    }
}

function initSpecCounters(gsap) {
    const counterElements = document.querySelectorAll('.spec-value[data-counter-target]');
    counterElements.forEach(el => {
        const targetStr = el.getAttribute('data-counter-target');
        const targetVal = parseFloat(targetStr);
        if (isNaN(targetVal)) return;

        const prefix = el.getAttribute('data-counter-prefix') || '';
        const suffix = el.getAttribute('data-counter-suffix') || '';
        const isDecimal = targetStr.includes('.');

        const counterObj = { val: 0 };
        gsap.to(counterObj, {
            val: targetVal,
            duration: 1.8,
            ease: 'power2.out',
            scrollTrigger: window.ScrollTrigger ? {
                trigger: el,
                start: 'top 92%',
                once: true
            } : undefined,
            onUpdate: () => {
                const text = prefix + (isDecimal ? counterObj.val.toFixed(1) : Math.round(counterObj.val)) + suffix;
                if (el.firstChild && el.firstChild.nodeType === 3) {
                    el.firstChild.nodeValue = text;
                } else {
                    el.textContent = text;
                }
            }
        });
    });
}

export function disposeLandingAnimations() {
    if (landingTimeline) {
        landingTimeline.kill();
        landingTimeline = null;
    }
    if (scrollTriggers.length > 0) {
        scrollTriggers.forEach(st => st.kill && st.kill());
        scrollTriggers = [];
    }
    if (parallaxTweens.length > 0) {
        parallaxTweens.forEach(pt => pt.kill && pt.kill());
        parallaxTweens = [];
    }
    if (cardTiltListeners.length > 0) {
        cardTiltListeners.forEach(({ card, onPointerMove, onPointerLeave }) => {
            card.removeEventListener('pointermove', onPointerMove);
            card.removeEventListener('pointerleave', onPointerLeave);
        });
        cardTiltListeners = [];
    }
    if (cursorGlowListener) {
        window.removeEventListener('pointermove', cursorGlowListener.onPointerMove);
        document.removeEventListener('mouseleave', cursorGlowListener.onPointerLeave);
        cursorGlowListener = null;
    }
}

function loadScript(src) {
    return new Promise((resolve, reject) => {
        if (document.querySelector(`script[src="${src}"]`)) {
            resolve();
            return;
        }
        const script = document.createElement('script');
        script.src = src;
        script.async = true;
        script.onload = () => resolve();
        script.onerror = () => reject(new Error(`Failed to load script ${src}`));
        document.head.appendChild(script);
    });
}
