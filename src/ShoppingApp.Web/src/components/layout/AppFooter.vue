<script setup>
</script>

<template>
  <footer class="app-footer">
    <div class="footer-links">
      <RouterLink to="/">Home</RouterLink>
      <span>|</span>
      <a href="#">About Us</a>
      <span>|</span>
      <a href="#">Contact Us</a>
    </div>
    <p class="footer-copyright">
      © {{ new Date().getFullYear() }} Minimal Shop. All rights reserved.
    </p>
  </footer>
</template>
