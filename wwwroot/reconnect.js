// Enhances the built-in Blazor Server reconnection UI (see Components/Layout/ReconnectModal.razor).
//
// Blazor toggles a state class on #components-reconnect-modal when the circuit drops. This script
// watches that element and:
//   * on "components-reconnect-failed" (built-in retries exhausted) -> keep retrying automatically
//     with a short delay, up to MAX_AUTO_RETRIES, so a briefly-idle or backgrounded tab recovers
//     on its own without the user clicking anything.
//   * on "components-reconnect-rejected" (server already released the circuit) -> a reload is the
//     only way back, so reload automatically after a short pause.
//   * on reconnect ("components-reconnect-hide") -> reset the retry counter.

(function () {
    var MAX_AUTO_RETRIES = 10;   // extra automatic attempts after Blazor's own retries give up
    var RETRY_DELAY_MS = 2000;   // wait between our automatic retries
    var RELOAD_DELAY_MS = 3000;  // pause before auto-reloading when the circuit is gone

    function wire() {
        var modal = document.getElementById('components-reconnect-modal');
        if (!modal) {
            // The component may not be in the DOM yet; try again shortly.
            setTimeout(wire, 500);
            return;
        }

        var autoRetries = 0;
        var reloadQueued = false;

        var observer = new MutationObserver(function () {
            var cls = modal.className || '';

            if (cls.indexOf('components-reconnect-hide') !== -1) {
                autoRetries = 0; // healthy again
                return;
            }

            if (cls.indexOf('components-reconnect-failed') !== -1) {
                if (autoRetries < MAX_AUTO_RETRIES) {
                    autoRetries++;
                    setTimeout(function () {
                        try {
                            if (window.Blazor && typeof window.Blazor.reconnect === 'function') {
                                window.Blazor.reconnect();
                            }
                        } catch (e) {
                            // ignore; the observer will fire again on the next state change
                        }
                    }, RETRY_DELAY_MS);
                } else if (!reloadQueued) {
                    // We've tried enough times; fall back to a reload.
                    reloadQueued = true;
                    setTimeout(function () { location.reload(); }, RELOAD_DELAY_MS);
                }
                return;
            }

            if (cls.indexOf('components-reconnect-rejected') !== -1) {
                if (!reloadQueued) {
                    reloadQueued = true;
                    setTimeout(function () { location.reload(); }, RELOAD_DELAY_MS);
                }
            }
        });

        observer.observe(modal, { attributes: true, attributeFilter: ['class'] });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', wire);
    } else {
        wire();
    }
})();
