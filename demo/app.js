const App = new Vue({
  el: '#vue-app',
  data: {
    app_name: 'minipeer',
    peer_id: null,
    server_url: null,
    client: null,
    clientConnected: false,
    peerConnected: false
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
      })
    },
    dataReceived: function (d) {
      console.log('data: ', d)
    }
  }
})
