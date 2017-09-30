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
    c_message: null
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
      this.client.sendTo(peerId, '>>>').then(() => {
        // successfully connected
        this.peerConnected = true
        this.connectedPeer = peerId
        console.log(`connected to ${peerId}!`)
      })
    },
    dataReceived: function (d) {
      console.log('data: ', d.data)
      if (!this.peerConnected) {
        if (d.data === '>>>') {
          // connect request
          console.log(`connection from peer ${d.source}`)
          // accept connection
          this.peerConnected = true
          this.connectedPeer = d.source
        }
      } else {
        // message received
        this.messages.push(d.data)
      }
    },
    sendMessage: function () {
      // send message
      let msg = this.c_message
      this.c_message = '' // clear message
      this.messages.push(msg)
      // send to peer
      this.client.sendTo(this.connectedPeer, msg)
    }
  }
})
