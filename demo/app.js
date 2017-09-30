const App = new Vue({
  el: '#vue-app',
  data: {
    app_name: 'minipeer',
    peer_id: null,
    server_url: null,
    client: null,
    clientConnected: false,
    peerConnected: false,
    connectedPeer: null,
    messages: [],
    c_message: null,
    chat_name: null,
    peer_chat_name: null
  },
  methods: {
    connectClient: function () {
      let url = this.server_url
      MiniPeer.create(url).then((peer) => {
        this.client = peer
        this.client.onReceive(this.dataReceived)
        this.client.onReady(() => {
          console.log(`connected to server with peer id ${peer.peerId}`)
          this.clientConnected = true
        })
      })
    },
    connectPeer: function () {
      let peerId = this.peer_id
      console.log(`connecting to ${peerId}`)
      this.client.sendTo(peerId, '>>>' + this.chat_name).then(() => {
        // successfully connected
        this.peerConnected = true
        this.connectedPeer = peerId
        console.log(`connected to ${peerId}!`)
      })
    },
    dataReceived: function (d) {
      console.log('data: ', d.data)
      if (!this.peerConnected) {
        if (d.data.startsWith('>>>')) {
          // connect request
          console.log(`connection from peer ${d.source}`)
          // accept connection
          this.peer_chat_name = d.data.substring(3)
          this.peerConnected = true
          this.connectedPeer = d.source
          // send chat name
          this.client.sendTo(this.connectedPeer, '<<<' + this.chat_name)
        }
      } else if (d.data.startsWith('<<<')) {
        this.peer_chat_name = d.data.substring(3)
      } else {
        // message received
        this.messages.push({
          m: d.data,
          n: this.peer_chat_name
        })
      }
    },
    sendMessage: function () {
      // send message
      let msg = this.c_message
      this.c_message = '' // clear message
      this.messages.push({
        m: msg,
        n: this.chat_name
      })
      // send to peer
      this.client.sendTo(this.connectedPeer, msg)
    }
  }
})
