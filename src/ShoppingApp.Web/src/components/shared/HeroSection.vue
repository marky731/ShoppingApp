<script setup>
import { RouterLink } from 'vue-router'
</script>

<template>
  <section class="hero-section">
    <h1 class="hero-title">Discover Your Style</h1>
    <p class="hero-subtitle">Minimal designs for a modern life.</p>
    <RouterLink to="/products" class="hero-btn">
      Shop Now
    </RouterLink>
  </section>
</template>
