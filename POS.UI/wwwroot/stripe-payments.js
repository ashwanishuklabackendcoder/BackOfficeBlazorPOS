window.stripePayments = (function () {
    let stripe = null;
    let card = null;
    let mounted = false;

    async function init(publishableKey, elementId) {
        if (!publishableKey) {
            throw new Error("Stripe publishable key is missing.");
        }

        if (!stripe) {
            stripe = Stripe(publishableKey);
        }

        if (mounted) return;

        const elements = stripe.elements();
        card = elements.create("card", { hidePostalCode: true });
        card.mount("#" + elementId);
        mounted = true;
    }

    async function confirmCardPayment(clientSecret) {
        if (!stripe || !card) {
            return { success: false, message: "Stripe is not initialized.", paymentIntentId: null, status: null };
        }

        const result = await stripe.confirmCardPayment(clientSecret, {
            payment_method: { card: card }
        });

        if (result.error) {
            return {
                success: false,
                message: result.error.message || "Card payment failed.",
                paymentIntentId: null,
                status: null
            };
        }

        return {
            success: true,
            message: null,
            status: result.paymentIntent ? result.paymentIntent.status : null,
            paymentIntentId: result.paymentIntent ? result.paymentIntent.id : null
        };
    }

    async function unmount() {
        if (card && mounted) {
            card.unmount();
            mounted = false;
        }
    }

    return {
        init,
        confirmCardPayment,
        unmount
    };
})();
