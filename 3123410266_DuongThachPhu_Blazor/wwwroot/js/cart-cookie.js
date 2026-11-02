export function getCart() {
    const match = document.cookie.match(/(?:^|; )cart=([^;]*)/);
    return match ? decodeURIComponent(match[1]) : null;
}

export function setCart(json) {
    const maxAge = 60 * 60 * 24 * 30; // 30 days
    document.cookie = `cart=${encodeURIComponent(json)}; path=/; max-age=${maxAge}; samesite=lax`;
}
