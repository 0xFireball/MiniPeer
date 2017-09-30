class MiniPeer {
  constructor(url) {
    this.url = url
  }

  static create(url) {
    let inst = new MiniPeer(url)
    return new Promise((resolve, reject) => {
      // get peer id
      inst.connect(() => {
        // success
        resolve(inst)
      }, (e) => {
        // failure
        reject('websocket error')
      })
    })
  }

  connect(s, f) {
    this.ws = new WebSocket(this.url)
    this.ws.onopen = s
    this.ws.onerror = f
    this.ws.onmessage = (e) => {
      let bundle = JSON.parse(e.data)
      if (bundle['type'] === 'data') {
        if (this.recv_cb) {
          this.recv_cb(bundle['data'])
        }
      } else if (bundle['success'] !== undefined) {
        let success = bundle['success']
        this.send_status(success)
      } else if (bundle['id']) {
        this.peerId = bundle['id']
        this.ready = true
        if (this.ready_cb) this.ready_cb()
      }
    }
  }

  sendTo(peerId, data) {
    let b = {
      target: peerId,
      data: data
    }
    return new Promise((resolve, reject) => {
      this.send_status = (v) => {
        if (v) {
          resolve()
        } else {
          reject(`send data to ${peerId} failed`)
        }
      }
      this.ws.send(JSON.stringify(b))
    })
  }

  onReceive(cb) {
    this.recv_cb = cb
  }

  onReady(cb) {
    this.ready_cb = cb
    if (this.ready) {
      cb()
    }
  }
}