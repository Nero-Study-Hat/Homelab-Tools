import { defineConfig } from 'vitepress'

// https://vitepress.dev/reference/site-config
export default defineConfig({
  srcDir: "pages",
  
  title: "Docs | Ansible Configs Tool",
  description: "Documentation for a custom C# tool used for centralized role and relevant host variable creation as well as automating boiler plate.",
  themeConfig: {
    // https://vitepress.dev/reference/default-theme-config
    nav: [
      { text: 'Home', link: '/' }
    ],

    // show H2 and H3 headings in outline
    outline: 'deep',

    sidebar: [
      {
        text: 'Core',
        items: [
          { text: 'Purpose', link: '/' },
          { text: 'Usage', link: '/how_it_works/usage' },
          { text: 'Config', link: '/how_it_works/config' }
        ]
      },
      {
        text: 'Extra Info',
        items: [
          { text: 'Notes', link: '/how_it_works/additional_notes' },
          { text: 'Configs', link: '/examples/base_config_examples' }
        ]
      }
    ],

    socialLinks: [
      { icon: 'github', link: 'https://github.com/vuejs/vitepress' }
    ]
  }
})
